/** @type {import('tailwindcss').Config} */
module.exports = {
    content: ["**/*.cshtml"],
    darkMode: "class",
    theme: {
        extend: {},
    },
    plugins: [
        "@tailwindcss/forms",
    ],
}

