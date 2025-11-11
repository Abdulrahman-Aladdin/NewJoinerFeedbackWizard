#!/usr/bin/env python3
import os, requests, math, time
from requests.auth import HTTPBasicAuth

# --- read & sanitize env vars
SONAR_HOST = os.environ.get('SONAR_HOST_URL', '').strip().rstrip('/')
SONAR_TOKEN = os.environ.get('SONAR_TOKEN', '').strip()
GITHUB_TOKEN = os.environ.get('GITHUB_TOKEN', '').strip()  # provided by Actions
REPO = os.environ.get('REPO', '').strip()  # owner/repo
SONAR_PROJECT_KEY = os.environ.get('SONAR_PROJECT_KEY', '').strip()  # optional

if not SONAR_HOST or not SONAR_TOKEN or not GITHUB_TOKEN or not REPO:
    raise SystemExit("Missing required env vars: SONAR_HOST_URL, SONAR_TOKEN, GITHUB_TOKEN, REPO")

owner, repo = REPO.split('/')

# If project key not provided, try to guess (owner_repo)
if not SONAR_PROJECT_KEY:
    SONAR_PROJECT_KEY = f"{owner}_{repo}"

SONAR_AUTH = HTTPBasicAuth(SONAR_TOKEN, '')

# --- Sonar fetching
def fetch_sonar_issues(project_key, page=1, page_size=500):
    url = f"{SONAR_HOST}/api/issues/search"
    params = {
        'componentKeys': project_key,
        'statuses': 'OPEN,REOPENED',
        'types': 'BUG,VULNERABILITY,CODE_SMELL',
        'ps': page_size,
        'p': page
    }
    r = requests.get(url, params=params, auth=SONAR_AUTH, timeout=30)
    r.raise_for_status()
    return r.json()

# --- GitHub helpers (use repo issues listing, NOT Search API)
def list_github_sonar_issues_map(state='open', per_page=100):
    """
    Return a map: sonar_key -> issue_number for issues labeled 'sonar'.
    """
    url = f"https://api.github.com/repos/{owner}/{repo}/issues"
    headers = {'Authorization': f'token {GITHUB_TOKEN}', 'Accept': 'application/vnd.github.v3+json'}
    page = 1
    issues_map = {}  # sonar_key -> issue number (first found)
    while True:
        r = requests.get(url, params={'state': state, 'labels': 'sonar', 'per_page': per_page, 'page': page}, headers=headers, timeout=30)
        r.raise_for_status()
        batch = r.json()
        if not batch:
            break
        for ish in batch:
            body = ish.get('body') or ''
            # extract sonar key from body
            for line in body.splitlines():
                if line.strip().startswith('sonar-issue-key:'):
                    key = line.strip().split(':',1)[1].strip()
                    if key and key not in issues_map:
                        issues_map[key] = ish.get('number')
                    break
        if len(batch) < per_page:
            break
        page += 1
    return issues_map

def create_github_issue(title, body, labels=None):
    url = f"https://api.github.com/repos/{owner}/{repo}/issues"
    headers = {'Authorization': f'token {GITHUB_TOKEN}', 'Accept': 'application/vnd.github.v3+json'}
    payload = {'title': title, 'body': body}
    if labels:
        payload['labels'] = labels
    r = requests.post(url, json=payload, headers=headers, timeout=20)
    r.raise_for_status()
    return r.json()

def list_github_sonar_issues(state='open', per_page=100):
    # kept if needed elsewhere
    url = f"https://api.github.com/repos/{owner}/{repo}/issues"
    headers = {'Authorization': f'token {GITHUB_TOKEN}', 'Accept': 'application/vnd.github.v3+json'}
    page = 1
    results = []
    while True:
        r = requests.get(url, params={'state': state, 'labels': 'sonar', 'per_page': per_page, 'page': page}, headers=headers, timeout=20)
        r.raise_for_status()
        batch = r.json()
        if not batch:
            break
        results.extend(batch)
        if len(batch) < per_page:
            break
        page += 1
    return results

def close_github_issue(number):
    url = f"https://api.github.com/repos/{owner}/{repo}/issues/{number}"
    headers = {'Authorization': f'token {GITHUB_TOKEN}', 'Accept': 'application/vnd.github.v3+json'}
    r = requests.patch(url, json={'state': 'closed'}, headers=headers, timeout=20)
    r.raise_for_status()
    return r.json()

def main():
    print("Using Sonar project key:", SONAR_PROJECT_KEY)
    created = 0
    page = 1
    page_size = 500
    sonar_open_keys = set()

    # Build map of existing GH sonar issues once
    existing_map = list_github_sonar_issues_map(state='open')
    print("Found", len(existing_map), "existing GH sonar issues")

    # 1) fetch Sonar open issues (collect keys)
    while True:
        data = fetch_sonar_issues(SONAR_PROJECT_KEY, page=page, page_size=page_size)
        issues = data.get('issues', [])
        for it in issues:
            key = it.get('key')
            sonar_open_keys.add(key)
            rule = it.get('rule')
            severity = it.get('severity')
            message = it.get('message') or ''
            component = it.get('component')
            line = it.get('line') or 'N/A'
            title = f"[Sonar] {severity}: {message[:80]} ({rule})"
            body = (
                f"**SonarQube finding**\n\n"
                f"- **Key:** {key}\n"
                f"- **Rule:** {rule}\n"
                f"- **Severity:** {severity}\n"
                f"- **Component:** {component}\n"
                f"- **Line:** {line}\n"
                f"- **Message:** {message}\n\n"
                f"sonar-issue-key:{key}\n\n"
                "(Automatically created from SonarQube analysis)"
            )
            # avoid duplicates using existing_map
            if key in existing_map:
                print("Already exists on GH:", key, "-> issue#", existing_map[key])
                continue
            # create issue and update map
            try:
                res = create_github_issue(title, body, labels=['sonar'])
                issue_num = res.get('number')
                existing_map[key] = issue_num
                print("Created GH issue:", res.get('html_url'))
                created += 1
            except Exception as e:
                print("Failed to create issue for", key, "error:", e)
                # continue to next issue without aborting whole run
                continue

        total = data.get('total', 0)
        # pagination break
        if page * page_size >= total:
            break
        page += 1
        time.sleep(0.2)

    print(f"Created {created} new GitHub issues from Sonar findings.")

    # 2) close GH issues whose sonar-key no longer present in Sonar open issues
    gh_issues = list_github_sonar_issues(state='open')
    closed = 0
    for ish in gh_issues:
        body = ish.get('body') or ''
        # find sonar key marker in body
        marker = None
        for line in body.splitlines():
            if line.strip().startswith('sonar-issue-key:'):
                marker = line.strip().split(':',1)[1].strip()
                break
        if not marker:
            continue
        if marker not in sonar_open_keys:
            print("Closing GH issue", ish.get('number'), "because sonar key", marker, "not found open")
            try:
                close_github_issue(ish.get('number'))
                closed += 1
            except Exception as e:
                print("Failed to close issue", ish.get('number'), "error:", e)
            time.sleep(0.2)
    print(f"Closed {closed} GitHub issues that were resolved in Sonar.")

if __name__ == "__main__":
    main()
