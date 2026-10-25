/* ========================================================================
   FICHALITY — EDITOR DINÂMICO
   Compatível com FichaController e classes do editor.css
   ======================================================================== */

let fichaAtual = {
    id: null,
    nomePersonagem: "",
    sistemaRpg: "",
    blocos: []
};
let alteracoesPendentes = false;

const $ = id => document.getElementById(id);

document.addEventListener("DOMContentLoaded", async () => {
    const params = new URLSearchParams(window.location.search);
    const id = Number(params.get("id"));
    if (!Number.isInteger(id) || id < 1) {
        window.location.replace("/home.html");
        return;
    }
    const btnVisualizar = $("btnVisualizar");
    if (btnVisualizar) btnVisualizar.href = `/visualizar-ficha.html?id=${encodeURIComponent(id)}`;
    $("nomePersonagem")?.addEventListener("input", marcarAlteracao);
    await carregarFichaPorId(id);
});

window.addEventListener("beforeunload", event => {
    if (!alteracoesPendentes) return;
    event.preventDefault();
    event.returnValue = "";
});

async function carregarFichaPorId(id) {
    try {
        const response = await fetch(`/api/ficha/${encodeURIComponent(id)}`, {
            credentials: "same-origin"
        });
        if (response.status === 401) {
            window.location.replace("/index.html");
            return;
        }
        if (!response.ok) throw new Error(await mensagemResposta(response, "Não foi possível carregar a ficha."));

        const data = await response.json();
        fichaAtual.id = data.id;
        fichaAtual.nomePersonagem = data.nomePersonagem || "";
        fichaAtual.sistemaRpg = data.sistemaRpg || "";

        try {
            fichaAtual.blocos = JSON.parse(data.estruturaJson || "[]");
            if (!Array.isArray(fichaAtual.blocos)) fichaAtual.blocos = [];
        } catch {
            throw new Error("A estrutura JSON da ficha está inválida.");
        }

        $("nomePersonagem").value = fichaAtual.nomePersonagem;
        const sistemaElement = $("sistemaRpg");
        if (sistemaElement) {
            if ("value" in sistemaElement) sistemaElement.value = fichaAtual.sistemaRpg;
            else sistemaElement.textContent = fichaAtual.sistemaRpg || "Sistema não informado";
        }
        renderizarEditor();
        alteracoesPendentes = false;
        atualizarStatus("Ficha carregada.", false);
    } catch (error) {
        console.error("Erro ao carregar ficha:", error);
        atualizarStatus(error.message || "Erro ao carregar a ficha.", true);
    }
}

function renderizarEditor() {
    const container = $("blocosContainer");
    if (!container) return;
    container.replaceChildren();

    if (fichaAtual.blocos.length === 0) {
        const vazio = document.createElement("div");
        vazio.className = "empty-state";
        const titulo = document.createElement("h3");
        titulo.textContent = "Sua ficha começa aqui";
        const texto = document.createElement("p");
        texto.textContent = "Adicione um bloco para começar a organizar os atributos, habilidades e informações do personagem.";
        vazio.append(titulo, texto);
        container.appendChild(vazio);
        return;
    }

    fichaAtual.blocos.forEach((bloco, bIndex) => {
        if (!Array.isArray(bloco.campos)) bloco.campos = [];
        const blocoEl = document.createElement("section");
        blocoEl.className = "bloco-card";

        const header = document.createElement("div");
        header.className = "bloco-header";

        const titulo = document.createElement("input");
        titulo.type = "text";
        titulo.className = "input-titulo";
        titulo.placeholder = "Título do bloco";
        titulo.value = bloco.titulo || "";
        titulo.setAttribute("aria-label", `Título do bloco ${bIndex + 1}`);
        titulo.addEventListener("input", () => {
            fichaAtual.blocos[bIndex].titulo = titulo.value;
            marcarAlteracao();
        });

        const acoes = document.createElement("div");
        acoes.className = "bloco-acoes";
        const subir = criarBotao("▲", "Mover bloco para cima", () => moverBloco(bIndex, -1));
        const descer = criarBotao("▼", "Mover bloco para baixo", () => moverBloco(bIndex, 1));
        const excluir = criarBotao("Excluir", "Excluir bloco", () => removerBloco(bIndex));
        excluir.classList.add("btn-del");
        subir.disabled = bIndex === 0;
        descer.disabled = bIndex === fichaAtual.blocos.length - 1;
        acoes.append(subir, descer, excluir);
        header.append(titulo, acoes);

        const camposContainer = document.createElement("div");
        camposContainer.className = "campos-container";

        bloco.campos.forEach((campo, cIndex) => {
            const linha = document.createElement("div");
            linha.className = "campo-linha";

            const chave = document.createElement("input");
            chave.type = "text";
            chave.placeholder = "Nome (ex.: Força)";
            chave.value = campo.chave || "";
            chave.setAttribute("aria-label", `Nome do campo ${cIndex + 1}`);
            chave.addEventListener("input", () => {
                fichaAtual.blocos[bIndex].campos[cIndex].chave = chave.value;
                marcarAlteracao();
            });

            const valor = document.createElement("input");
            valor.type = "text";
            valor.placeholder = "Valor (ex.: 18)";
            valor.value = campo.valor ?? "";
            valor.setAttribute("aria-label", `Valor do campo ${cIndex + 1}`);
            valor.addEventListener("input", () => {
                fichaAtual.blocos[bIndex].campos[cIndex].valor = valor.value;
                marcarAlteracao();
            });

            const remover = criarBotao("×", "Remover campo", () => removerCampo(bIndex, cIndex));
            remover.className = "btn-del-sm";
            linha.append(chave, valor, remover);
            camposContainer.appendChild(linha);
        });

        const adicionar = document.createElement("button");
        adicionar.type = "button";
        adicionar.className = "btn-add-sm";
        adicionar.textContent = "+ Adicionar Campo";
        adicionar.addEventListener("click", () => adicionarCampo(bIndex));

        blocoEl.append(header, camposContainer, adicionar);
        container.appendChild(blocoEl);
    });
}

function criarBotao(texto, ariaLabel, handler) {
    const button = document.createElement("button");
    button.type = "button";
    button.textContent = texto;
    button.setAttribute("aria-label", ariaLabel);
    button.addEventListener("click", handler);
    return button;
}

function adicionarBloco() {
    fichaAtual.blocos.push({
        titulo: "Novo Bloco",
        campos: [{ chave: "Campo", valor: "Valor" }]
    });
    renderizarEditor();
    marcarAlteracao();
}

function removerBloco(index) {
    if (!confirm("Excluir este bloco e todos os seus campos?")) return;
    fichaAtual.blocos.splice(index, 1);
    renderizarEditor();
    marcarAlteracao();
}

function moverBloco(index, direcao) {
    const novoIndex = index + direcao;
    if (novoIndex < 0 || novoIndex >= fichaAtual.blocos.length) return;
    [fichaAtual.blocos[index], fichaAtual.blocos[novoIndex]] =
        [fichaAtual.blocos[novoIndex], fichaAtual.blocos[index]];
    renderizarEditor();
    marcarAlteracao();
}

function adicionarCampo(bIndex) {
    if (!Array.isArray(fichaAtual.blocos[bIndex].campos)) {
        fichaAtual.blocos[bIndex].campos = [];
    }
    fichaAtual.blocos[bIndex].campos.push({ chave: "", valor: "" });
    renderizarEditor();
    marcarAlteracao();
}

function removerCampo(bIndex, cIndex) {
    fichaAtual.blocos[bIndex].campos.splice(cIndex, 1);
    renderizarEditor();
    marcarAlteracao();
}

function marcarAlteracao() {
    alteracoesPendentes = true;
    atualizarStatus("Alterações não salvas", true);
}

function atualizarStatus(mensagem, erro) {
    // Se a página ainda não tem indicador de status, usa console como fallback.
    const status = $("saveStatusText");
    if (status) status.textContent = mensagem;
    else if (erro) console.warn(mensagem);
}

async function salvarEstruturaFicha() {
    if (!fichaAtual.id) {
        alert("Carregue uma ficha antes de salvar.");
        return;
    }

    const nomePersonagem = $("nomePersonagem")?.value.trim();
    if (!nomePersonagem) {
        alert("Informe o nome do personagem.");
        return;
    }

    const botaoSalvar = Array.from(document.querySelectorAll("button"))
        .find(button => button.textContent.includes("Salvar no Banco"));
    if (botaoSalvar) { botaoSalvar.disabled = true; botaoSalvar.textContent = "Salvando..."; }

    try {
        // Salva os metadados editáveis primeiro.
        const dadosResponse = await fetch(`/api/ficha/${fichaAtual.id}`, {
            method: "PUT",
            credentials: "same-origin",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                nomePersonagem,
                sistemaRpg: fichaAtual.sistemaRpg
            })
        });
        if (!dadosResponse.ok) throw new Error(await mensagemResposta(dadosResponse, "Não foi possível salvar os dados básicos."));

        // Salva a estrutura dinâmica em JSON.
        const estruturaResponse = await fetch("/api/ficha/salvar-estrutura", {
            method: "PUT",
            credentials: "same-origin",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                fichaId: fichaAtual.id,
                estruturaJson: JSON.stringify(fichaAtual.blocos)
            })
        });
        if (!estruturaResponse.ok) throw new Error(await mensagemResposta(estruturaResponse, "Não foi possível salvar a estrutura."));

        fichaAtual.nomePersonagem = nomePersonagem;
        alteracoesPendentes = false;
        atualizarStatus("Todas as alterações foram salvas.", false);
        alert("Ficha salva com sucesso!");
    } catch (error) {
        console.error("Erro ao salvar ficha:", error);
        alert(error.message || "Erro ao salvar a ficha.");
    } finally {
        if (botaoSalvar) { botaoSalvar.disabled = false; botaoSalvar.textContent = "💾 Salvar no Banco"; }
    }
}

async function mensagemResposta(response, fallback) {
    const texto = await response.text();
    if (!texto) return `${fallback} (HTTP ${response.status})`;
    try {
        const dados = JSON.parse(texto);
        return dados.mensagem || dados.title || dados.detail || fallback;
    } catch {
        return texto;
    }
}

// Expostas para manter compatibilidade com o onclick do HTML existente.
window.carregarFichaPorId = carregarFichaPorId;
window.renderizarEditor = renderizarEditor;
window.adicionarBloco = adicionarBloco;
window.removerBloco = removerBloco;
window.moverBloco = moverBloco;
window.adicionarCampo = adicionarCampo;
window.removerCampo = removerCampo;
window.salvarEstruturaFicha = salvarEstruturaFicha;
