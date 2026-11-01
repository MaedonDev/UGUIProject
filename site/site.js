// 网站交互：侧栏、搜索、代码块（切换 / 复制）
const $ = (s, r = document) => r.querySelector(s);
const $$ = (s, r = document) => [...r.querySelectorAll(s)];

// ---------- 侧栏（窄屏抽屉）与当前小节高亮 ----------
$('.page-main')?.addEventListener('click', () => document.body.classList.remove('nav-open'));
const secLinks = $$('.nav-sec a');
if (secLinks.length) {
  const map = new Map(secLinks.map((a) => [a.getAttribute('href').slice(1), a]));
  const io = new IntersectionObserver((es) => {
    for (const e of es) if (e.isIntersecting) {
      secLinks.forEach((a) => a.classList.remove('on'));
      const a = map.get(e.target.id);
      if (a) { a.classList.add('on'); a.scrollIntoView({ block: 'nearest' }); }
    }
  }, { rootMargin: '-60px 0px -70% 0px' });
  $$('h3[id]').forEach((h) => io.observe(h));
}
$('.nav-ch.on')?.scrollIntoView({ block: 'center' });

// ---------- 搜索 ----------
const box = $('.search input'), res = $('.search .results');
let index = null, sel = -1;
async function loadIndex() { if (!index) index = await (await fetch(rootPath() + 'assets/search.json')).json(); return index; }
function rootPath() { const s = document.querySelector('script[src*="assets/site.js"]').getAttribute('src'); return s.slice(0, s.indexOf('assets/site.js')) || './'; }
box?.addEventListener('focus', loadIndex);
box?.addEventListener('input', async () => {
  const q = box.value.trim().toLowerCase();
  const idx = await loadIndex();
  if (!q) { res.classList.remove('on'); return; }
  const words = q.split(/\s+/);
  const hits = idx.filter((x) => words.every((w) => (x.t + ' ' + x.s).toLowerCase().includes(w))).slice(0, 40);
  res.innerHTML = hits.length ? hits.map((h) => `<a href="${rootPath()}${h.u}">${escapeHtml(h.t)}<small>${escapeHtml(h.s)}</small></a>`).join('') : '<a>没有找到</a>';
  res.classList.add('on');
  sel = -1;
});
box?.addEventListener('keydown', (e) => {
  const items = $$('a', res);
  if (e.key === 'ArrowDown' || e.key === 'ArrowUp') { e.preventDefault(); sel = (sel + (e.key === 'ArrowDown' ? 1 : -1) + items.length) % items.length; items.forEach((a, i) => a.classList.toggle('sel', i === sel)); }
  if (e.key === 'Enter' && items[Math.max(sel, 0)]?.href) location.href = items[Math.max(sel, 0)].href;
  if (e.key === 'Escape') { res.classList.remove('on'); box.blur(); }
});
document.addEventListener('click', (e) => { if (!e.target.closest('.search')) res?.classList.remove('on'); });
function escapeHtml(s) { return s.replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c])); }

// ---------- 代码块 ----------
for (const cb of $$('.codebox')) {
  const tabs = $$('.cb-tabs button', cb), panes = $$('.cb-pane', cb);
  tabs.forEach((t, i) => t.addEventListener('click', () => {
    tabs.forEach((x, k) => x.classList.toggle('on', k === i));
    panes.forEach((x, k) => x.classList.toggle('on', k === i));
  }));
  $('.cb-copy', cb)?.addEventListener('click', async (e) => {
    const pane = $('.cb-pane.on', cb) || cb;
    await navigator.clipboard.writeText($('pre', pane).innerText);
    e.target.textContent = '已复制';
    setTimeout(() => (e.target.textContent = '复制'), 1200);
  });
}
