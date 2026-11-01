// 网站检查：本地起服务打开 web/ 的页面，桌面（1440）和手机（420）宽度、浅色和深色各截一张图，并报告横向溢出和脚本错误。
// 用法: node tools/sitecheck.mjs <输出目录> [页面…]      （playwright 借用 books/godot/tools/node_modules，tools/node_modules 是指过去的链接）
//   也可以检查线上: SITE=https://gametech.byte888.com/ugui/ node tools/sitecheck.mjs <输出目录> index.html
import { chromium } from 'playwright';
import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = path.join(path.dirname(fileURLToPath(import.meta.url)), '..', 'web');
const [out, ...pages] = process.argv.slice(2);
const list = pages.length ? pages : ['index.html', 'chapters/06-button-and-selectable.html', 'cases/rounded-rect-graphic.html', 'source.html'];
fs.mkdirSync(out, { recursive: true });
const MIME = { '.html': 'text/html; charset=utf-8', '.js': 'text/javascript', '.json': 'application/json', '.png': 'image/png', '.jpg': 'image/jpeg', '.css': 'text/css', '.svg': 'image/svg+xml' };
const server = http.createServer((req, res) => {
  let p = decodeURIComponent(req.url.split('?')[0]).replace(/^\/ugui/, '');
  if (p.endsWith('/')) p += 'index.html';
  const f = path.join(ROOT, p);
  if (!f.startsWith(ROOT) || !fs.existsSync(f)) { res.writeHead(404); res.end(); return; }
  res.writeHead(200, { 'Content-Type': MIME[path.extname(f)] || 'application/octet-stream' });
  fs.createReadStream(f).pipe(res);
}).listen(0);
const base = process.env.SITE || `http://localhost:${server.address().port}/ugui/`;

const browser = await chromium.launch({ channel: 'chrome', headless: true });
let bad = 0;
for (const [w, h, tag] of [[1440, 1000, 'desk'], [420, 900, 'mob']]) {
  for (const scheme of ['light', 'dark']) {
    const ctx = await browser.newContext({ viewport: { width: w, height: h }, colorScheme: scheme, deviceScaleFactor: 1 });
    const page = await ctx.newPage();
    const errs = [];
    page.on('pageerror', (e) => errs.push(e.message));
    page.on('console', (m) => { if (m.type() === 'error' && !/googletagmanager|hm\.baidu|adsbygoogle|pagead|net::ERR|api\/ai\/usage/.test(m.text())) errs.push(m.text()); });
    for (const p of list) {
      await page.goto(base + p, { waitUntil: 'domcontentloaded', timeout: 30000 });
      await page.waitForTimeout(1500);
      const over = await page.evaluate(() => document.documentElement.scrollWidth - innerWidth);
      const name = `${tag}-${scheme}-${p.replace(/[\/.]/g, '_')}.png`;
      await page.screenshot({ path: path.join(out, name) });
      const msg = [over > 1 ? `横向溢出 ${over}px` : '', errs.length ? `错误: ${errs.join(' | ')}` : ''].filter(Boolean).join('；');
      if (msg) bad++;
      console.log(`${msg ? '✗' : '✓'} ${tag} ${scheme} ${p}${msg ? '  ' + msg : ''}`);
      errs.length = 0;
    }
    await ctx.close();
  }
}
await browser.close();
server.close();
process.exit(bad ? 1 : 0);
