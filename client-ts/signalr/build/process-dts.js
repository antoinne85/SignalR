const fs = require('fs');
const path = require('path');
const rimraf = require('rimraf');

// Load package.json
const root = path.resolve(__dirname, "..");
const pkg = require(path.resolve(root, 'package.json'));

// Copy typings, except index.d.ts (we're going to edit it)
const typingsSource = path.resolve(root, "obj", "typings");
const typingsDest = path.resolve(root, path.dirname(pkg.typings));
const typingsBaseName = path.basename(pkg.typings);

if(fs.existsSync(typingsDest)) {
    rimraf.sync(typingsDest);
}
fs.mkdirSync(typingsDest);

fs.readdirSync(typingsSource).forEach(file => {
    if(file !== "index.d.ts") {
        fs.copyFileSync(path.join(typingsSource, file), path.join(typingsDest, file));
    }
});

// Add "export as namespace signalR" to index.d.ts, which allows it to be used in browser-targetting files as a global
let indexContent = fs.readFileSync(path.join(typingsSource, "index.d.ts")).toString();
fs.writeFileSync(path.join(typingsDest, typingsBaseName), indexContent + `\r\nexport as namespace ${pkg.umd_name};`);