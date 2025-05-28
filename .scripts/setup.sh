#!/bin/bash
[ -f .env ] || exit 0
# Auto-restore project tools if needed
command -v puml-gen >/dev/null 2>&1 || dotnet tool restore >/dev/null 2>&1
PROFILE="${HOME}/$([[ "$SHELL" == *zsh* ]] && echo .zshrc || echo .bashrc)"
while IFS='=' read -r key value; do
    [ "$key" = "GODOT_BIN" ] && {
        grep -q "^export GODOT_BIN=" "$PROFILE" && {
            old_path=$(dirname "$(grep "^export GODOT_BIN=" "$PROFILE" | cut -d'=' -f2)")
            old_path_export="export PATH=\"$old_path:\$PATH\""
            grep -q "^$old_path_export" "$PROFILE" && sed -i.bak "\|^$old_path_export|d" "$PROFILE"
        }
        echo "export PATH=\"$(dirname "$value"):\$PATH\"" >> "$PROFILE"
        export PATH="$(dirname "$value"):$PATH"
    }
    grep -q "^export $key=" "$PROFILE" && sed -i.bak "/^export $key=/d" "$PROFILE"
    echo "export $key=$value" >> "$PROFILE"
    export "$key=$value"
done < <(grep -vE '^#|^$' .env)
source "$PROFILE"
echo "Environment loaded to $PROFILE."