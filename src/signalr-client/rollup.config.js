import pkg from "./package.json";
import babel from "rollup-plugin-babel";
import cleanup from "rollup-plugin-cleanup";
import typescript from "rollup-plugin-typescript2";
import sourcemaps from "rollup-plugin-sourcemaps";

export default [
    // Browser-friendly UMD
    {
        input: "./src/index.ts",
        output: {
            file: pkg.browser,
            format: "umd",
            name: "signalR",
            sourcemap: true,
        },
        plugins: [
            typescript({
                typescript: require('typescript'),
                useTsconfigDeclarationDir: true
            }),
            cleanup(),
            sourcemaps(),
        ]
    },

    // CommonJS and ES2015 module
    {
        input: "./src/index.ts",
        output: [
            { file: pkg.main, format: "cjs", sourcemap: true },
            { file: pkg.module, format: "es", sourcemap: true },
        ],
        plugins: [
            typescript({
                typescript: require('typescript'),
                useTsconfigDeclarationDir: true,
                tsconfigOverride: {
                    compilerOptions: {
                        target: "es2016"
                    }
                }
            }),
            cleanup(),
            sourcemaps(),
        ]
    }
]