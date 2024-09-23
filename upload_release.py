﻿import sys
import os

from github import Github, Auth


path, system = sys.argv[1], sys.argv[2]


with open("version.txt") as f:
    version = f.read().strip().replace('\ufeff', '')

print(f"Version = {repr(version)}")


# using an access token
auth = Auth.Token(os.getenv("GITHUB_TOKEN"))

# Public Web GitHub
g = Github(auth=auth)

repo = g.get_repo('SergeiKrivko/GPT-chat-cs')

release = repo.get_latest_release()
print(repr(release.tag_name), version)
if release.tag_name != "v" + version:
    release = repo.create_git_release("v" + version, "Version 1.0.1", '')

release.upload_asset(path, name=f"GPT-chat-{system}")

if sys.platform == 'win32':
    repo.create_git_release('v0.0.1', "Version 0.0.1", '')
