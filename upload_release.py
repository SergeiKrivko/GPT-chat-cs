import sys
import os

from github import Github, Auth


path, system = sys.argv[1], sys.argv[2]


with open("version.txt") as f:
    version = f.read().strip()


# using an access token
auth = Auth.Token(os.getenv("GITHUB_TOKEN"))

# Public Web GitHub
g = Github(auth=auth)

repo = g.get_user().get_repo('GPT-chat-cs')

release = repo.get_latest_release()
if release.tag_name != f"v{version}":
    release = repo.create_git_release(f"v{version}", f"Version {version}", '')

release.upload_asset(path, name=f"GPT-chat-{system}")
