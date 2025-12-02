#!/bin/bash
# usage: ./migrations.sh add Initial
cmd=$1
if [ -z "$cmd" ]; then
  echo "Usage: ./migrations.sh add <Name> | update"
  exit 1
fi
if [ "$cmd" = "add" ]; then
  name=$2
  dotnet ef migrations add $name --project src/UserDocs.Infrastructure --startup-project src/UserDocs.API
elif [ "$cmd" = "update" ]; then
  dotnet ef database update --project src/UserDocs.Infrastructure --startup-project src/UserDocs.API
else
  echo "Unknown command"
fi
