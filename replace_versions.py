with open("version.txt") as f:
    version = f.read().strip()


for file in ["Utils/Config.cs", "Releases/win-x64/setup.iss", "Releases/mac-x64/GPT-chat-avalonia.app/Contents/Info.plist"]:
    with open(file, 'r') as f:
        text = f.read()
        
    text = text.replace("{AppVersion}", version)
    
    with open(file, 'w') as f:
            f.write(text)
