# Info tab content (`info-page.md`)

- The **Info** tab loads **`src/Assets/info-page.md`** at **build time** as a WPF `Resource` (it is not shipped as a loose file next to `Ordir.exe`).
- It is **not** full Markdown. A subset is implemented in **`InfoTab.xaml.cs`** (bold, italic, `#` / `##` / `###` headings, `---`, `-` bullets, backtick code, and `http`/`https` links in `[label](url)` form). **Only** lines that begin with `# `, `## `, or `### ` (after leading spaces) are rendered as headings; a normal sentence that ends with a colon is body text.
- **Promo buttons** after **Find this useful?** are **intentionally not** in `info-page.md`. Their URLs and image lookup live in **`InfoTab.xaml.cs`** so layout and assets stay consistent.

Edit `src/Assets/info-page.md`, then rebuild the app so the embedded copy updates.
