import path from "path";

import pkg from "./package.json";
import sourcemaps from "rollup-plugin-sourcemaps";
import typescript from "rollup-plugin-typescript2";

const root = __dirname;
const srcDir = path.join(root, "src");
const input = path.join(srcDir, "index.ts");

const typescriptOptions = {
    useTsconfigDeclarationDir: true
}

export default [
    // Browser-friendly UMD
    {
        input: input,
        output: {
            file: pkg.browser,
            format: "umd",
            name: pkg.umd_name,
            sourcemap: true,
        },
        plugins: [
            typescript({
                ...typescriptOptions,
                tsconfigOverride: {
                    compilerOptions: {
                        target: "es5"
                    }
                }
            }),
            sourcemaps(),
        ]
    },

    // CommonJS and ES2015 module
    {
        input: input,
        output: [
            { file: pkg.main, format: "cjs", sourcemap: true },
            { file: pkg.module, format: "es", sourcemap: true },
        ],
        plugins: [
            typescript(typescriptOptions),
            sourcemaps(),
        ]
    }
]
