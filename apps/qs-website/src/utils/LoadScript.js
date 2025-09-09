export default function loadScript(src, id) {
  if (document.getElementById(id)) return; // avoid duplicates
  const script = document.createElement("script");
  script.src = src;
  script.async = true;
  script.id = id;
  document.body.appendChild(script);
}
