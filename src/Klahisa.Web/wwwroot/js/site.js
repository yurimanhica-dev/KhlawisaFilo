(function () {
  var wrapper = document.getElementById("appWrapper");
  var toggle = document.getElementById("sidebarToggle");
  var overlay = document.getElementById("sidebarOverlay");

  function lerDecimal(valor) {
    var texto = String(valor == null ? "" : valor).trim().replace(/,/g, "");
    if (!texto) return 0;
    var numero = Number(texto);
    return Number.isFinite(numero) ? numero : NaN;
  }

  function formatarDecimal(campo) {
    if (!campo || !campo.matches("[data-decimal-input]")) return;
    var valor = String(campo.value || "").trim();
    if (!valor) return;
    var numero = lerDecimal(valor);
    if (Number.isFinite(numero)) {
      campo.value = new Intl.NumberFormat("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(numero);
    }
  }

  function lerDataCampo(campo) {
    if (!campo || campo.type !== "date") return null;
    var partes = String(campo.value || "").match(/^(\d{4})-(\d{2})-(\d{2})$/);
    if (!partes) return null;
    var ano = Number(partes[1]);
    var mes = Number(partes[2]) - 1;
    var dia = Number(partes[3]);
    var data = new Date(ano, mes, dia, 12);
    return data.getFullYear() === ano && data.getMonth() === mes && data.getDate() === dia ? data : null;
  }

  function paraDataCampo(data) {
    return data.getFullYear() + "-" + String(data.getMonth() + 1).padStart(2, "0") + "-" + String(data.getDate()).padStart(2, "0");
  }

  function prepararCampoData(campo) {
    if (!campo || campo.type !== "date") return;
    campo.setCustomValidity("");
  }


  document.querySelectorAll("[data-decimal-input]").forEach(function (campo) {
    function validarDecimal() {
      var valor = String(campo.value || "").trim();
      var numero = lerDecimal(valor);
      var minimo = campo.dataset.minValue === undefined ? null : Number(campo.dataset.minValue);
      if (valor && !Number.isFinite(numero)) campo.setCustomValidity("Introduza um valor numérico válido.");
      else if (valor && minimo !== null && numero < minimo) campo.setCustomValidity("O valor não pode ser negativo.");
      else campo.setCustomValidity("");
    }
    campo.addEventListener("focus", function () { campo.value = campo.value.replace(/,/g, ""); });
    campo.addEventListener("input", validarDecimal);
    campo.addEventListener("blur", function () { validarDecimal(); formatarDecimal(campo); });
    validarDecimal();
    formatarDecimal(campo);
  });

  document.querySelectorAll('input[type="date"]').forEach(function (campo) {
    campo.addEventListener("input", function () { prepararCampoData(campo); });
    campo.addEventListener("blur", function () { prepararCampoData(campo); });
    prepararCampoData(campo);
  });

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

  document.querySelectorAll('form[data-client-type-form="true"]').forEach(function (form) {
    var tipo = form.querySelector('[name="Tipo"]');
    if (!tipo) return;

    function actualizarCamposCliente() {
      form.querySelectorAll("[data-show-for]").forEach(function (campo) {
        var mostrar = !campo.dataset.showFor || campo.dataset.showFor === tipo.value;
        campo.classList.toggle("is-hidden", !mostrar);
        var control = campo.querySelector("input, select, textarea");
        if (control && campo.dataset.requiredFor) control.required = mostrar;
        var indicador = campo.querySelector(".required-marker");
        if (indicador) indicador.hidden = !mostrar;
      });

      form.querySelectorAll("[data-tab-for-type]").forEach(function (tab) {
        var mostrar = !tab.dataset.tabForType || tab.dataset.tabForType === tipo.value;
        tab.classList.toggle("is-hidden", !mostrar);
        tab.setAttribute("aria-hidden", mostrar ? "false" : "true");
      });
      var abaAtiva = form.querySelector("[data-form-tab].is-active");
      if (abaAtiva && abaAtiva.classList.contains("is-hidden")) {
        var primeiraAbaVisivel = form.querySelector("[data-form-tab]:not(.is-hidden)");
        if (primeiraAbaVisivel) primeiraAbaVisivel.click();
      }
    }

    tipo.addEventListener("change", actualizarCamposCliente);
    actualizarCamposCliente();
  });

  document.querySelectorAll("select[data-depends-on]").forEach(function (select) {
    if (!select.dataset.dependsOn) return;
    var form = select.closest("form");
    var parent = form && form.querySelector('[name="' + select.dataset.dependsOn + '"]');
    if (!parent) return;

    function actualizarOpcoes(resetInvalid) {
      var pai = parent.value;
      var selecao = select.value;
      select.querySelectorAll("option").forEach(function (option) {
        if (!option.value) { option.hidden = false; return; }
        var corresponde = option.dataset.parentId === pai;
        option.hidden = !corresponde;
        option.disabled = !corresponde;
        if (option.value === selecao && !corresponde && resetInvalid) select.value = "";
      });
      select.disabled = !pai;
    }

    parent.addEventListener("change", function () { actualizarOpcoes(true); });
    select.addEventListener("change", function () {
      form.querySelectorAll('select[data-depends-on="' + select.name + '"]').forEach(function (filho) {
        var evento = new Event("change");
        filho.dispatchEvent(evento);
      });
    });
    actualizarOpcoes(false);
  });

  document.querySelectorAll('form[data-insurance-request="true"]').forEach(function (form) {
    var tipoSeguro = form.querySelector("[data-insurance-type='true']");
    function campo(nome) {
      return form.querySelector('[data-field-name="' + nome + '"]') || form.querySelector('[name="' + nome + '"]');
    }
    function definir(nome, valor) {
      var controlo = campo(nome);
      if (controlo) {
        controlo.value = valor == null ? "" : valor;
        formatarDecimal(controlo);
        prepararCampoData(controlo);
      }
      var oculto = form.querySelector('input[type="hidden"][name="' + nome + '"]');
      if (oculto) oculto.value = valor == null ? "" : valor;
    }
    function actualizarPremios() {
      var premioTotal = lerDecimal(campo("Premio")?.value || 0);
      var coeficiente = lerDecimal(campo("Coeficiente")?.value || 0);
      var periodicidade = campo("Periodicidade")?.value || "Anual";
      var periodosAno = { Anual: 1, Semestral: 2, Trimestral: 4, Mensal: 12 }[periodicidade] || 1;
      var encargoPct = lerDecimal(campo("EncargoPct")?.value || 0);
      var seloPct = lerDecimal(campo("SeloPct")?.value || 0);
      var sobretaxaPct = lerDecimal(campo("SobretaxaPct")?.value || 0);
      var comissaoPct = lerDecimal(campo("ComissaoPct")?.value || 0);
      var premioLiquido = premioTotal * coeficiente / periodosAno;
      var encargo = premioLiquido * encargoPct / 100;
      var selo = premioLiquido * seloPct / 100;
      var sobretaxa = premioLiquido * sobretaxaPct / 100;
      definir("PremioLiquido", premioLiquido.toFixed(2));
      definir("EncargoValor", encargo.toFixed(2));
      definir("SeloValor", selo.toFixed(2));
      definir("SobretaxaValor", sobretaxa.toFixed(2));
      definir("ComissaoValor", (premioLiquido * comissaoPct / 100).toFixed(2));
    }
    function preencherReferenciaSeguradora(apenasSeVazia) {
      var referencia = campo("ReferenciaSeguradora");
      if (!referencia || (apenasSeVazia && referencia.value)) return;
      var seguradora = campo("SeguradoraId");
      var opcao = seguradora && seguradora.selectedOptions[0];
      definir("ReferenciaSeguradora", opcao ? (opcao.dataset.optionData || "") : "");
    }
    function actualizarTipoSeguro(forcarReferencia) {
      var opcao = tipoSeguro && tipoSeguro.selectedOptions[0];
      var dados = opcao && opcao.dataset.optionData ? opcao.dataset.optionData.split("|") : [];
      definir("Ramo", dados[1] || "");
      definir("Coeficiente", dados[2] || "0.00");
      definir("EncargoPct", dados[3] || "0.00");
      definir("SeloPct", dados[4] || "0.00");
      definir("SobretaxaPct", dados[5] || "0.00");
      definir("ComissaoPct", dados[6] || "0.00");
      preencherReferenciaSeguradora(!forcarReferencia);
      actualizarPremios();
    }
    function actualizarCambio() {
      var moeda = campo("Moeda");
      var opcao = moeda && moeda.selectedOptions[0];
      definir("Cambio", opcao && opcao.dataset.optionData ? opcao.dataset.optionData : "");
    }
    function actualizarPeriodo() {
      var inicio = campo("Inicio");
      if (!inicio || !inicio.value) return;
      var dataInicio = lerDataCampo(inicio);
      if (!dataInicio) return;
      var anoRenovacao = dataInicio.getFullYear() + 1;
      var mesRenovacao = dataInicio.getMonth();
      var diaRenovacao = Math.min(dataInicio.getDate(), new Date(anoRenovacao, mesRenovacao + 1, 0).getDate());
      var renovacao = new Date(anoRenovacao, mesRenovacao, diaRenovacao, 12);
      var fim = new Date(renovacao);
      fim.setDate(fim.getDate() - 1);
      definir("Fim", paraDataCampo(fim));
      definir("DataRenovacao", paraDataCampo(renovacao));
    }
    if (tipoSeguro) tipoSeguro.addEventListener("change", function () { actualizarTipoSeguro(true); });
    var seguradora = campo("SeguradoraId");
    if (seguradora) seguradora.addEventListener("change", function () {
      preencherReferenciaSeguradora(false);
      if (tipoSeguro) tipoSeguro.dispatchEvent(new Event("change"));
    });
    ["Premio"].forEach(function (nome) {
      var controlo = campo(nome);
      if (controlo) controlo.addEventListener("input", actualizarPremios);
    });
    var moeda = campo("Moeda");
    if (moeda) moeda.addEventListener("change", actualizarCambio);
    var dataInicio = campo("Inicio");
    if (dataInicio) dataInicio.addEventListener("change", actualizarPeriodo);
    var periodicidade = campo("Periodicidade");
    if (periodicidade) periodicidade.addEventListener("change", actualizarPremios);
    actualizarTipoSeguro();
    actualizarCambio();
    actualizarPeriodo();
  });

  document.querySelectorAll("[data-search-select]").forEach(function (select) {
    var input = select.querySelector("[data-search-select-input]");
    var valor = select.querySelector("[data-search-select-value]");
    var menu = select.querySelector("[data-search-select-menu]");
    var toggle = select.querySelector("[data-search-select-toggle]");
    var empty = select.querySelector("[data-search-select-empty]");
    var opcoes = Array.from(select.querySelectorAll("[data-search-select-option]"));
    if (!input || !valor || !menu) return;

    function normalizar(texto) {
      return String(texto || "").toLocaleLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "").trim();
    }
    function abrir(mostrarTodas) {
      menu.hidden = false;
      input.setAttribute("aria-expanded", "true");
      filtrar(mostrarTodas);
    }
    function fechar() {
      menu.hidden = true;
      input.setAttribute("aria-expanded", "false");
      opcoes.forEach(function (opcao) { opcao.classList.remove("is-active"); });
      if (!input.value && valor.value) {
        var selecionada = opcoes.find(function (opcao) { return opcao.dataset.value === valor.value; });
        if (selecionada) input.value = selecionada.dataset.label;
      }
    }
    function filtrar(mostrarTodas) {
      var consulta = mostrarTodas ? "" : normalizar(input.value);
      var visiveis = opcoes.filter(function (opcao) {
        var corresponde = normalizar(opcao.dataset.label).includes(consulta);
        opcao.hidden = !corresponde;
        opcao.classList.remove("is-active");
        return corresponde;
      });
      if (empty) empty.hidden = visiveis.length > 0;
      if (visiveis.length) visiveis[0].classList.add("is-active");
      if (!mostrarTodas) {
        var exacta = opcoes.find(function (opcao) { return normalizar(opcao.dataset.label) === normalizar(input.value); });
        valor.value = exacta ? exacta.dataset.value : "";
      }
      input.setCustomValidity(valor.value ? "" : "Seleccione um resultado da lista.");
    }
    function seleccionar(opcao) {
      if (!opcao) return;
      input.value = opcao.dataset.label;
      valor.value = opcao.dataset.value;
      input.setCustomValidity("");
      opcoes.forEach(function (item) { item.setAttribute("aria-selected", item === opcao ? "true" : "false"); });
      fechar();
    }
    function moverActivo(direccao) {
      var visiveis = opcoes.filter(function (opcao) { return !opcao.hidden; });
      if (!visiveis.length) return;
      var actual = visiveis.findIndex(function (opcao) { return opcao.classList.contains("is-active"); });
      var seguinte = Math.max(0, Math.min(visiveis.length - 1, actual + direccao));
      visiveis.forEach(function (opcao, indice) { opcao.classList.toggle("is-active", indice === seguinte); });
      visiveis[seguinte].scrollIntoView({ block: "nearest" });
    }

    input.addEventListener("focus", function () {
      if (valor.value) {
        input.select();
        abrir(true);
      } else abrir();
    });
    input.addEventListener("click", function () { if (menu.hidden) abrir(); });
    input.addEventListener("input", function () { abrir(); filtrar(); });
    input.addEventListener("keydown", function (event) {
      if (event.key === "ArrowDown") {
        event.preventDefault();
        var estavaFechado = menu.hidden;
        abrir();
        if (!estavaFechado) moverActivo(1);
      }
      else if (event.key === "ArrowUp") {
        event.preventDefault();
        var estavaFechado = menu.hidden;
        abrir();
        if (estavaFechado) {
          var ultima = opcoes.filter(function (opcao) { return !opcao.hidden; }).pop();
          opcoes.forEach(function (opcao) { opcao.classList.toggle("is-active", opcao === ultima); });
        } else moverActivo(-1);
      }
      else if (event.key === "Enter" && !menu.hidden) {
        event.preventDefault();
        seleccionar(opcoes.find(function (opcao) { return !opcao.hidden && opcao.classList.contains("is-active"); }));
      } else if (event.key === "Escape") fechar();
    });
    if (toggle) toggle.addEventListener("click", function () {
      if (menu.hidden) {
        input.focus();
        abrir(true);
      } else fechar();
    });
    opcoes.forEach(function (opcao) {
      opcao.addEventListener("mousedown", function (event) { event.preventDefault(); });
      opcao.addEventListener("click", function () { seleccionar(opcao); });
    });
    document.addEventListener("click", function (event) { if (!select.contains(event.target)) fechar(); });
    var selecionada = opcoes.find(function (opcao) { return opcao.dataset.value === valor.value; });
    if (selecionada) {
      input.value = selecionada.dataset.label;
      input.setCustomValidity("");
      selecionada.setAttribute("aria-selected", "true");
    } else filtrar();
  });

  document.querySelectorAll('form[data-validate-tabs="true"]').forEach(function (form) {
    var tabs = Array.from(form.querySelectorAll("[data-form-tab]"));
    var panels = Array.from(form.querySelectorAll("[data-form-tab-panel]"));

    function activarAba(index) {
      tabs.forEach(function (tab, i) {
        var activo = i === index;
        tab.classList.toggle("is-active", activo);
        tab.setAttribute("aria-selected", activo ? "true" : "false");
      });
      panels.forEach(function (panel, i) { panel.classList.toggle("is-active", i === index); });
    }

    var aviso = form.querySelector("[data-required-warning]");
    var temporizadorAviso;
    function mostrarAviso(campo) {
      if (!aviso) return;
      if (campo) {
        var grupo = campo.closest(".form-group");
        var rotulo = grupo && grupo.querySelector("label");
        if (rotulo) aviso.textContent = "Preencha ou corrija o campo: " + rotulo.textContent.replace(/\*/g, "").trim() + ".";
      }
      aviso.hidden = false;
      window.clearTimeout(temporizadorAviso);
      temporizadorAviso = window.setTimeout(function () { aviso.hidden = true; }, 5000);
    }
    if (aviso && !aviso.hidden) mostrarAviso();
    tabs.forEach(function (tab) {
      tab.addEventListener("click", function () {
        var indiceDestino = Number(tab.dataset.formTab);
        var indiceActual = tabs.findIndex(function (item) { return item.classList.contains("is-active"); });
        if (indiceDestino > indiceActual) {
          for (var i = indiceActual; i < indiceDestino; i++) {
            var obrigatorioInvalido = panels[i] && Array.from(panels[i].querySelectorAll("input[required], select[required], textarea[required]"))
              .find(function (campo) { return !campo.disabled && !campo.checkValidity(); });
            if (obrigatorioInvalido) {
              mostrarAviso(obrigatorioInvalido);
              activarAba(i);
              window.setTimeout(function () { obrigatorioInvalido.focus(); }, 0);
              return;
            }
          }
        }
        activarAba(indiceDestino);
      });
    });

    function esconderAvisoSeValido() {
      if (aviso && !form.querySelector("input:invalid, select:invalid, textarea:invalid")) {
        aviso.hidden = true;
        window.clearTimeout(temporizadorAviso);
      }
    }
    form.addEventListener("input", esconderAvisoSeValido);
    form.addEventListener("change", esconderAvisoSeValido);

    form.addEventListener("submit", function (event) {
      var invalido = form.querySelector("input:invalid, select:invalid, textarea:invalid");
      if (invalido) {
        event.preventDefault();
        var estado = invalido.validity;
        console.warn("[Klahisa] Validação do formulário", {
          campo: invalido.name || invalido.dataset.fieldName || "sem nome",
          tipo: invalido.type || invalido.tagName.toLowerCase(),
          obrigatorio: invalido.required,
          comprimento: (invalido.value || "").length,
          valueMissing: estado.valueMissing,
          typeMismatch: estado.typeMismatch,
          patternMismatch: estado.patternMismatch,
          badInput: estado.badInput,
          customError: estado.customError,
          valid: estado.valid
        });
        mostrarAviso(invalido);
        var painel = invalido.closest("[data-form-tab-panel]");
        if (painel) {
          ativarAba(Number(painel.dataset.formTabPanel));
          window.setTimeout(function () { invalido.focus(); }, 0);
        }
        return;
      }

      var botao = event.submitter || form.querySelector('button[type="submit"]');
      if (botao) {
        botao.disabled = true;
        botao.classList.add("is-loading");
        botao.innerHTML = '<span class="button-spinner" aria-hidden="true"></span>A gravar...';
      }
    });
  });

  document.querySelectorAll("form").forEach(function (form) {
    form.addEventListener("submit", function (event) {
      if (event.defaultPrevented) return;
      var camposNumericos = Array.from(form.querySelectorAll("[data-decimal-input]"));
      var camposData = Array.from(form.querySelectorAll('input[type="date"]'));
      var invalido = camposNumericos.concat(camposData).find(function (campo) { return !campo.checkValidity(); });
      if (invalido) {
        event.preventDefault();
        invalido.focus();
        invalido.reportValidity();
        return;
      }
      camposNumericos.forEach(function (campo) { campo.value = campo.value.replace(/,/g, ""); });
    });
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
