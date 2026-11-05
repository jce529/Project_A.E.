#!/usr/bin/env bash
# GSD 설치/업데이트가 머신 고유 절대경로(D:/..., C:/Users/...)를 .claude/.gemini/.agents 에 박아 넣는지 검사한다.
# 사용: scripts/check-gsd-paths.sh          -> 발견 시 목록 출력 후 exit 1
#       scripts/check-gsd-paths.sh --fix    -> 프로젝트 루트 상대경로(./.claude/...)로 치환
set -euo pipefail
cd "$(git rev-parse --show-toplevel)"

DIRS=(.claude .gemini .agents)
# 드라이브 문자 + 프로젝트 루트(.claude|.gemini|.agents 앞까지)를 잡는다. 사용자 홈(~/.claude)의 절대경로도 포함.
PATTERN='[A-Za-z]:[/\\]+[^"'"'"' )`]*[/\\]+\.(claude|gemini|agents)[/\\]'

hits=$(git grep -lIE "$PATTERN" -- "${DIRS[@]}" ':!*/settings.local.json' || true)
if [ -z "$hits" ]; then echo "OK: 절대경로 없음"; exit 0; fi

if [ "${1:-}" = "--fix" ]; then
  while IFS= read -r f; do
    sed -i -E 's#[A-Za-z]:[/\\]+[^"'"'"' )`]*[/\\]+(\.(claude|gemini|agents))[/\\]#./\1/#g' "$f"
  done <<< "$hits"
  echo "FIXED: $(echo "$hits" | wc -l) files"; exit 0
fi

echo "절대경로가 남아 있는 파일:"; echo "$hits"; exit 1
