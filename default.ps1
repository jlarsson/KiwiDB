properties {
	$base_dir = resolve-path .
	$build_configuration = "Release"
	$build_platform = "Any CPU"
	$build_dir = join-path $base_dir "build\4.0\"
	$build_properties = "OutDir=$build_dir;Configuration=$build_configuration;Platform=$build_platform"
	$pack_dir = join-path $base_dir "packs"
	$solutions = ("KiwiDb.sln")
	$kiwidb = Create-Project-Description `
		-base_dir $base_dir `
		-project_name "KiwiDb" `
		-id "KiwiDb" `
		-author "Joakim Larsson" `
		-description "KiwiDB embedded document database manager" `
		-copyright "Copyright © Joakim Larsson 2011" `

	$projects = ($kiwidb)
}



$framework = '4.0'

include .\psake_ext.ps1

task default -depends pack

task clean {
	remove-item -force -recurse $build_dir -ErrorAction SilentlyContinue
}

task update-assembly-info -depends clean {
	$projects | foreach { (Generate-Assembly-Info $_) }
}

task build -depends update-assembly-info {
	$solutions | foreach { msbuild $_ /target:Build /nologo /verbosity:quiet /p:$build_properties }
}

task pack -depends build {
	Ensure-Directory $pack_dir
	$projects | foreach { `
		$p = $_
		$props = "id=$($p.id);version=$($p.version);title=$($p.title);author=$($p.author);description=$($p.description);copyright=$($p.copyright)"
		(.nuget\nuget pack $p.nuspec_file `
			-p $props `
			-basePath $base_dir `
			-o $pack_dir) `
	}
}
