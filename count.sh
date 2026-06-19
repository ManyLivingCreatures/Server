#!/bin/sh

cloc ./* \
	--exclude-dir="bin,obj,node_modules,.git,FrontendJS,Useless" \
	--not-match-d="Migrations|Generated|dist" \
	--not-match-f="(.*\.gitea\/.*|.*node_modules\/.*|.*\.pnpm\/.*|.*\.g\.dart)|\.dart_tool" \
	--not-match-d="flutter_app/build|flutter_app/android|flutter_app/ios|flutter_app/windows|flutter_app/web" \
	--include-lang="C#,TypeScript,JavaScript,Vuejs Component,SCSS,Protocol Buffers,HTML,Dart,CSS,F#,TOML"
