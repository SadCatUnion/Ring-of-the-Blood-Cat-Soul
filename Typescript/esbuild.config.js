// todo merge copyJsFile.js
var fs = require('fs');
var path = require('path');

function copyFileSync( source, target ) {

    var targetFile = target;

    //if target is a directory a new file with the same name will be created
    if ( fs.existsSync( target ) ) {
        if ( fs.lstatSync( target ).isDirectory() ) {
            targetFile = path.join( target, path.basename( source ) + '.txt' );
        }
    }

    fs.writeFileSync(targetFile, fs.readFileSync(source));
}

function copyFolderRecursiveSync( source, targetFolder ) {
    var files = [];

    if ( !fs.existsSync( targetFolder ) ) {
        fs.mkdirSync( targetFolder );
    }

    //copy
    if ( fs.lstatSync( source ).isDirectory() ) {
        files = fs.readdirSync( source );
        files.forEach( function ( file ) {
            var curSource = path.join( source, file );
            if ( fs.lstatSync( curSource ).isDirectory() ) {
                copyFolderRecursiveSync( curSource, path.join( targetFolder, file) );
            } else {
                copyFileSync( curSource, targetFolder );
            }
        } );
    }
}

require('esbuild').build({
    entryPoints: ['GameRoot.ts'],
    bundle: true,
    format: 'cjs',
    outfile: 'outPut/GameRoot.js',
    external: ['csharp'],
    watch: {
        onRebuild(error, result) {
          if (error) console.error('watch build failed:', error)
          else {
            console.log('watch build succeeded:', result)
            copyFolderRecursiveSync("outPut", "../Assets/StreamingAssets")
          }
        },
      },
}).then(result => {
    console.log('watching...')
})