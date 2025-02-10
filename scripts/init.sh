#!/bin/bash

# Configure git to use the tracked hooks directory instead of $GIT_DIR/hooks
git config core.hooksPath hooks

# Add git bash command to global git configuration
git config --global "alias.bash" "!sh $1"

# Pull $1 into branch $2 and push to origin
git config --global "alias.promote" "!sh -c 'git pull --all && git checkout $1 && git push && git checkout $2 && git pull . $1 && git push && git checkout $1' $1 $2"


