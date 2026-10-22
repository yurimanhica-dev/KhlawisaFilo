(function () {
  var wrapper = document.getElementById("appWrapper");
  var toggle = document.getElementById("sidebarToggle");
  var overlay = document.getElementById("sidebarOverlay");

  if (toggle && wrapper) {
    toggle.addEventListener("click", function () {
      wrapper.classList.toggle(window.innerWidth >= 992 ? "sidebar-hidden" : "sidebar-open");
    });
  }
  if (overlay) overlay.addEventListener("click", function () { wrapper.classList.remove("sidebar-open"); });

  document.querySelectorAll("[data-dd]").forEach(function (b) {
    b.addEventListener("click", function (e) {
      e.stopPropagation();
      var m = b.parentElement.querySelector(".dd-menu");
      var abrir = !m.classList.contains("open");
      document.querySelectorAll(".dd-menu.open").forEach(function (x) { x.classList.remove("open"); });
      if (abrir) m.classList.add("open");
    });
  });
  document.addEventListener("click", function (e) {
    if (!e.target.closest(".dd-menu")) document.querySelectorAll(".dd-menu.open").forEach(function (x) { x.classList.remove("open"); });
  });
  document.addEventListener("keydown", function (e) {
    if (e.key === "Escape") {
      wrapper && wrapper.classList.remove("sidebar-open");
      document.querySelectorAll(".dd-menu.open").forEach(function (x) { x.classList.remove("open"); });
    }
  });

  document.querySelectorAll("form[data-confirm]").forEach(function (f) {
    if (!f.dataset.confirm) return;
    f.addEventListener("submit", function (e) { if (!confirm(f.dataset.confirm)) e.preventDefault(); });
  });

  document.querySelectorAll("[data-table-search]").forEach(function (i) {
    i.addEventListener("input", function () {
      var q = i.value.toLowerCase();
      i.closest(".card").querySelectorAll("tbody tr").forEach(function (r) {
        r.style.display = r.classList.contains("row-head") || r.textContent.toLowerCase().indexOf(q) > -1 ? "" : "none";
      });
    });
  });
})();
