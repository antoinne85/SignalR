const fs = require('fs');
const path = require('path');
const rimraf = require('rimraf');

// Load package.json
const root = path.resolve(__dirname, "..");
const pkg = require(path.resolve(root, 'package.json'));

// Just copy typings
const typingsSource = path.resolve(root, "obj", "typings", "signalr-protocol-msgpack", "src");
const typingsDest = path.resolve(root, path.dirname(pkg.typings));
const typingsBaseName = path.basename(pkg.typings);

if(fs.existsSync(typingsDest)) {
    rimraf.sync(typingsDest);
}
fs.mkdirSync(typingsDest);

fs.readdirSync(typingsSource).forEach(file => {
    if(file !== "index.d.ts") {
        fs.copyFileSync(path.join(typingsSource, file), path.join(typingsDest, file));
    } else {
        fs.copyFileSync(path.join(typingsSource, file), path.join(typingsDest, typingsBaseName));
    }
});

// We don't do 'export as namespace' because of https://github.com/Microsoft/TypeScript/issues/20990