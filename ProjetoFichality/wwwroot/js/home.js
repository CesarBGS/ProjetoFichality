/* ==========================================================================
   FICHALITY — HOME
   Consome a API e encaminha fichas para a página de visualização.
   ========================================================================== */

document.addEventListener("DOMContentLoaded", inicializarHome);

let usuarioAtual = null;
let indiceDestaque = 0;
let destaques = [];

async function inicializarHome() {
    configurarEventos();

    try {
        const respostaUsuario = await fetch("/api/autenticacao/me", {
            credentials: "same-origin"
        });

        if (respostaUsuario.status === 401 || respostaUsuario.status === 403) {
            window.location.replace("/index.html");
            return;
        }
        if (!respostaUsuario.ok) {
            throw new Error(`Não foi possível validar a sessão (HTTP ${respostaUsuario.status}).`);
        }

        usuarioAtual = await respostaUsuario.json();
        document.getElementById("nomeUsuario").textContent = usuarioAtual.nome || "aventureiro";

        const resultados = await Promise.allSettled([carregarResumo(), carregarDestaques()]);
        if (resultados[0].status === "rejected") {
            console.error("Erro ao carregar o resumo:", resultados[0].reason);
            mostrarErroFichas("Não foi possível carregar suas fichas. Verifique se a API /api/home está disponível.");
        }
    } catch (erro) {
        console.error("Erro ao inicializar a HOME:", erro);
        mostrarErroFichas("Não foi possível carregar os dados. Atualize a página ou entre novamente.");
    }
}

function configurarEventos() {
    document.getElementById("btnNovaFicha")?.addEventListener("click", abrirDialogNovaFicha);
    document.getElementById("fecharDialog")?.addEventListener("click", fecharDialogNovaFicha);
    document.getElementById("formNovaFicha")?.addEventListener("submit", criarNovaFicha);
    document.getElementById("slideAnterior")?.addEventListener("click", () => mudarDestaque(-1));
    document.getElementById("slideProximo")?.addEventListener("click", () => mudarDestaque(1));
}

async function carregarResumo() {
    const resposta = await fetch("/api/home", { credentials: "same-origin" });
    if (resposta.status === 401 || resposta.status === 403) {
        window.location.replace("/index.html");
        return;
    }
    const dados = await lerJson(resposta);
    if (!resposta.ok) throw new Error(dados.mensagem || dados.title || `Falha ao carregar resumo (HTTP ${resposta.status}).`);

    document.getElementById("nomeUsuario").textContent = dados.usuario?.nome || usuarioAtual?.nome || "aventureiro";
    document.getElementById("totalFichas").textContent = dados.totalFichas ?? 0;
    document.getElementById("totalSistemas").textContent = dados.totalSistemasCriados ?? 0;
    renderizarFichas(Array.isArray(dados.fichasRecentes) ? dados.fichasRecentes : []);
}

function renderizarFichas(fichas) {
    const container = document.getElementById("fichasContainer");
    container.replaceChildren();

    if (fichas.length === 0) {
        const estado = document.createElement("div");
        estado.className = "empty-state";
        const titulo = document.createElement("h3");
        titulo.textContent = "Sua próxima aventura começa aqui";
        const texto = document.createElement("p");
        texto.textContent = "Você ainda não tem fichas. Crie seu primeiro personagem para começar.";
        const botao = document.createElement("button");
        botao.type = "button";
        botao.className = "btn btn-primary";
        botao.textContent = "Criar minha primeira ficha";
        botao.addEventListener("click", abrirDialogNovaFicha);
        estado.append(titulo, texto, botao);
        container.appendChild(estado);
        return;
    }

    for (const ficha of fichas) {
        const id = Number(ficha.id);
        if (!Number.isInteger(id) || id <= 0) continue;

        const link = document.createElement("a");
        link.className = "ficha-card";
        // A Home abre primeiro o modo de leitura; a edição é uma ação separada.
        link.href = `/visualizar-ficha.html?id=${encodeURIComponent(id)}`;
        link.setAttribute("aria-label", `Visualizar ficha ${ficha.nomePersonagem || "sem nome"}`);

        const simbolo = document.createElement("span");
        simbolo.className = "ficha-symbol";
        simbolo.setAttribute("aria-hidden", "true");
        simbolo.textContent = "✦";

        const detalhes = document.createElement("div");
        detalhes.className = "ficha-details";

        const titulo = document.createElement("h3");
        titulo.textContent = ficha.nomePersonagem || "Personagem sem nome";

        const sistema = document.createElement("p");
        sistema.textContent = `${ficha.sistema || ficha.sistemaRpg || "Sistema não informado"} · ${formatarData(ficha.dataAtualizacao)}`;

        detalhes.append(titulo, sistema);

        const seta = document.createElement("span");
        seta.className = "ficha-arrow";
        seta.setAttribute("aria-hidden", "true");
        seta.textContent = "›";

        link.append(simbolo, detalhes, seta);
        container.appendChild(link);
    }

    if (!container.childElementCount) {
        mostrarErroFichas("Nenhuma ficha válida foi retornada pela API.");
    }
}

function mostrarErroFichas(mensagem) {
    const container = document.getElementById("fichasContainer");
    if (!container) return;
    const aviso = document.createElement("p");
    aviso.className = "section-loading";
    aviso.textContent = mensagem;
    container.replaceChildren(aviso);
}

function formatarData(valor) {
    if (!valor) return "Data indisponível";
    const data = new Date(valor);
    if (Number.isNaN(data.getTime())) return "Data indisponível";
    return `Atualizada em ${data.toLocaleDateString("pt-BR")}`;
}

async function carregarDestaques() {
    const container = document.getElementById("carouselContainer");
    try {
        const resposta = await fetch("/api/destaques", { credentials: "same-origin" });
        const dados = await lerJson(resposta);
        if (!resposta.ok) throw new Error(dados.mensagem || dados.title || `HTTP ${resposta.status}`);
        destaques = Array.isArray(dados) ? dados : [];
        indiceDestaque = 0;
        renderizarDestaque();
    } catch (erro) {
        console.error("Erro ao carregar destaques:", erro);
        container.replaceChildren();
        const aviso = document.createElement("div");
        aviso.className = "carousel-placeholder";
        aviso.textContent = "Não foi possível carregar os destaques neste momento.";
        container.appendChild(aviso);
        document.getElementById("carouselIndicadores").replaceChildren();
    }
}

function mudarDestaque(direcao) {
    if (!destaques.length) return;
    indiceDestaque = (indiceDestaque + direcao + destaques.length) % destaques.length;
    renderizarDestaque();
}

function renderizarDestaque() {
    const container = document.getElementById("carouselContainer");
    const indicadores = document.getElementById("carouselIndicadores");
    container.replaceChildren();
    indicadores.replaceChildren();

    if (!destaques.length) {
        const slide = document.createElement("article");
        slide.className = "featured-slide";
        const conteudo = document.createElement("div");
        conteudo.className = "featured-content";
        const tag = document.createElement("span");
        tag.className = "featured-tag";
        tag.textContent = "Em breve";
        const titulo = document.createElement("h3");
        titulo.textContent = "Novidades do universo RPG";
        const texto = document.createElement("p");
        texto.textContent = "Os destaques aparecerão aqui quando houver conteúdos publicados.";
        conteudo.append(tag, titulo, texto);
        slide.appendChild(conteudo);
        container.appendChild(slide);
        return;
    }

    const destaque = destaques[indiceDestaque];
    const slide = document.createElement("article");
    slide.className = "featured-slide";

    const imagem = valor(destaque, "imagemUrl", "ImagemUrl");
    const imagemSegura = imagem ? validarUrl(imagem) : null;
    if (imagemSegura) {
        const img = document.createElement("img");
        img.className = "featured-slide-image";
        img.src = imagemSegura;
        img.alt = "";
        img.loading = "lazy";
        img.addEventListener("error", () => img.remove());
        slide.classList.add("has-image");
        slide.appendChild(img);
    }

    const conteudo = document.createElement("div");
    conteudo.className = "featured-content";
    const tag = document.createElement("span");
    tag.className = "featured-tag";
    tag.textContent = valor(destaque, "tipo", "Tipo") || "Destaque";
    const titulo = document.createElement("h3");
    titulo.textContent = valor(destaque, "titulo", "Titulo") || "Destaque do Fichality";
    const resumo = document.createElement("p");
    resumo.textContent = valor(destaque, "resumo", "Resumo") || "";
    conteudo.append(tag, titulo, resumo);

    const linkStr = valor(destaque, "linkUrl", "LinkUrl");
    const linkSeguro = linkStr ? validarUrl(linkStr) : null;
    if (linkSeguro) {
        const link = document.createElement("a");
        link.className = "featured-link";
        link.href = linkSeguro;
        link.textContent = "Conhecer destaque →";
        if (new URL(linkSeguro, window.location.origin).origin !== window.location.origin) {
            link.target = "_blank";
            link.rel = "noopener noreferrer";
        }
        conteudo.appendChild(link);
    } else {
        const sistemaId = Number(valor(destaque, "sistemaRpgId", "SistemaRpgId"));
        const modeloId = Number(valor(destaque, "modeloFichaId", "ModeloFichaId"));
        const destino = (sistemaId > 0 || modeloId > 0) ? "/sistemas.html" : null;
        if (destino) {
            const link = document.createElement("a");
            link.className = "featured-link";
            link.href = destino;
            link.textContent = "Conhecer destaque →";
            conteudo.appendChild(link);
        }
    }

    slide.appendChild(conteudo);
    container.appendChild(slide);

    destaques.forEach((_, indice) => {
        const botao = document.createElement("button");
        botao.type = "button";
        botao.className = indice === indiceDestaque ? "active" : "";
        botao.setAttribute("aria-label", `Exibir destaque ${indice + 1}`);
        botao.setAttribute("aria-current", indice === indiceDestaque ? "true" : "false");
        botao.addEventListener("click", () => {
            indiceDestaque = indice;
            renderizarDestaque();
        });
        indicadores.appendChild(botao);
    });
}

function valor(objeto, camelCase, pascalCase) {
    return objeto?.[camelCase] ?? objeto?.[pascalCase] ?? "";
}

function validarUrl(valorUrl) {
    try {
        const url = new URL(valorUrl, window.location.origin);
        if (url.protocol !== "https:" && url.protocol !== "http:") return null;
        return url.href;
    } catch {
        return null;
    }
}

function abrirDialogNovaFicha() {
    const dialog = document.getElementById("dialogNovaFicha");
    document.getElementById("erroNovaFicha").textContent = "";
    if (typeof dialog.showModal === "function") dialog.showModal();
    document.getElementById("novoNomePersonagem").focus();
}

function fecharDialogNovaFicha() {
    document.getElementById("dialogNovaFicha").close();
}

async function criarNovaFicha(evento) {
    evento.preventDefault();
    const botao = document.getElementById("confirmarNovaFicha");
    const erro = document.getElementById("erroNovaFicha");
    erro.textContent = "";

    const nomePersonagem = document.getElementById("novoNomePersonagem").value.trim();
    const sistemaRpg = document.getElementById("novoSistemaRpg").value.trim();
    if (!nomePersonagem || !sistemaRpg) {
        erro.textContent = "Preencha o nome do personagem e o sistema.";
        return;
    }

    botao.disabled = true;
    botao.textContent = "Criando...";
    try {
        const resposta = await fetch("/api/ficha/criar", {
            method: "POST",
            credentials: "same-origin",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                nomePersonagem,
                sistemaRpg,
                usuarioId: usuarioAtual?.id ?? 0
            })
        });
        const dados = await lerJson(resposta);
        if (!resposta.ok) throw new Error(dados.mensagem || dados.title || `Falha ao criar ficha (HTTP ${resposta.status}).`);
        const id = Number(dados.fichaId ?? dados.id);
        if (!Number.isInteger(id) || id <= 0) throw new Error("A API não retornou o ID da ficha criada.");
        window.location.assign(`/visualizar-ficha.html?id=${encodeURIComponent(id)}`);
    } catch (erroCriacao) {
        console.error("Erro ao criar ficha:", erroCriacao);
        erro.textContent = erroCriacao.message || "Não foi possível criar a ficha.";
    } finally {
        botao.disabled = false;
        botao.textContent = "Criar ficha";
    }
}

async function lerJson(resposta) {
    const texto = await resposta.text();
    if (!texto) return {};
    try { return JSON.parse(texto); }
    catch { return { mensagem: texto }; }
}
