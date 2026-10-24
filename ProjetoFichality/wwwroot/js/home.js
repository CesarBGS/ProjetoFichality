
/* ==========================================================================
   FICHALITY — HOME
   Dados reais consumidos da API ASP.NET Core
   ========================================================================== */

document.addEventListener("DOMContentLoaded", inicializarHome);

let usuarioAtual = null;
let indiceDestaque = 0;
let destaques = [];

// --------------------------------------------------------------------------
// Inicialização
// --------------------------------------------------------------------------

async function inicializarHome() {
    configurarEventos();

    try {
        const respostaUsuario = await fetch("/api/autenticacao/me", {
            credentials: "same-origin"
        });

        if (!respostaUsuario.ok) {
            window.location.replace("index.html");
            return;
        }

        usuarioAtual = await respostaUsuario.json();

        const nome = usuarioAtual.nome || "aventureiro";
        document.getElementById("nomeUsuario").textContent = nome;

        await Promise.all([
            carregarResumo(),
            carregarDestaques()
        ]);
    } catch (erro) {
        console.error("Erro ao inicializar a HOME:", erro);

        mostrarErroFichas(
            "Não foi possível carregar os dados. Tente atualizar a página."
        );
    }
}

// --------------------------------------------------------------------------
// Eventos da interface
// --------------------------------------------------------------------------

function configurarEventos() {
    document.getElementById("btnNovaFicha")
        ?.addEventListener("click", abrirDialogNovaFicha);

    document.getElementById("fecharDialog")
        ?.addEventListener("click", fecharDialogNovaFicha);

    document.getElementById("formNovaFicha")
        ?.addEventListener("submit", criarNovaFicha);

    document.getElementById("slideAnterior")
        ?.addEventListener("click", () => mudarDestaque(-1));

    document.getElementById("slideProximo")
        ?.addEventListener("click", () => mudarDestaque(1));
}

// --------------------------------------------------------------------------
// Resumo do usuário e fichas recentes
// --------------------------------------------------------------------------

async function carregarResumo() {
    const resposta = await fetch("/api/home", {
        credentials: "same-origin"
    });

    if (resposta.status === 401) {
        window.location.replace("index.html");
        return;
    }

    if (!resposta.ok) {
        throw new Error(`Falha ao carregar a HOME: HTTP ${resposta.status}`);
    }

    const dados = await resposta.json();

    document.getElementById("nomeUsuario").textContent =
        dados.usuario?.nome || usuarioAtual?.nome || "aventureiro";

    document.getElementById("totalFichas").textContent =
        dados.totalFichas ?? 0;

    document.getElementById("totalSistemas").textContent =
        dados.totalSistemasCriados ?? 0;

    renderizarFichas(dados.fichasRecentes || []);
}

function renderizarFichas(fichas) {
    const container = document.getElementById("fichasContainer");
    container.replaceChildren();

    if (!fichas.length) {
        const estado = document.createElement("div");
        estado.className = "empty-state";

        const titulo = document.createElement("h3");
        titulo.textContent = "Sua próxima aventura começa aqui";

        const texto = document.createElement("p");
        texto.textContent =
            "Você ainda não tem fichas. Crie seu primeiro personagem para começar.";

        const botao = document.createElement("button");
        botao.type = "button";
        botao.className = "btn btn-primary";
        botao.textContent = "Criar minha primeira ficha";
        botao.addEventListener("click", abrirDialogNovaFicha);

        estado.append(titulo, texto, botao);
        container.appendChild(estado);
        return;
    }

    fichas.forEach(ficha => {
        const link = document.createElement("a");
        link.className = "ficha-card";
        link.href = `editor.html?id=${encodeURIComponent(ficha.id)}`;

        const simbolo = document.createElement("span");
        simbolo.className = "ficha-symbol";
        simbolo.setAttribute("aria-hidden", "true");
        simbolo.textContent = "✦";

        const detalhes = document.createElement("div");
        detalhes.className = "ficha-details";

        const titulo = document.createElement("h3");
        titulo.textContent = ficha.nomePersonagem || "Personagem sem nome";

        const sistema = document.createElement("p");
        sistema.textContent =
            `${ficha.sistema || ficha.sistemaRpg || "Sistema não informado"} · ` +
            formatarData(ficha.dataAtualizacao);

        detalhes.append(titulo, sistema);

        const seta = document.createElement("span");
        seta.className = "ficha-arrow";
        seta.setAttribute("aria-hidden", "true");
        seta.textContent = "›";

        link.append(simbolo, detalhes, seta);
        container.appendChild(link);
    });
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

    if (Number.isNaN(data.getTime())) {
        return "Data indisponível";
    }

    return `Atualizada em ${data.toLocaleDateString("pt-BR")}`;
}

// --------------------------------------------------------------------------
// Carrossel de destaques
// --------------------------------------------------------------------------

async function carregarDestaques() {
    const container = document.getElementById("carouselContainer");

    try {
        const resposta = await fetch("/api/destaques");

        if (!resposta.ok) {
            throw new Error(`HTTP ${resposta.status}`);
        }

        destaques = await resposta.json();
        indiceDestaque = 0;

        renderizarDestaque();
    } catch (erro) {
        console.error("Erro ao carregar destaques:", erro);

        container.replaceChildren();

        const aviso = document.createElement("div");
        aviso.className = "carousel-placeholder";
        aviso.textContent =
            "Não foi possível carregar os destaques neste momento.";

        container.appendChild(aviso);
    }
}

function mudarDestaque(direcao) {
    if (!destaques.length) return;

    indiceDestaque =
        (indiceDestaque + direcao + destaques.length) % destaques.length;

    renderizarDestaque();
}

function renderizarDestaque() {
    const container = document.getElementById("carouselContainer");
    const indicadores = document.getElementById("carouselIndicadores");

    container.replaceChildren();
    indicadores.replaceChildren();

    if (!destaques.length) {
        const vazio = document.createElement("div");
        vazio.className = "featured-slide";

        const conteudo = document.createElement("div");
        conteudo.className = "featured-content";

        const etiqueta = document.createElement("span");
        etiqueta.className = "featured-tag";
        etiqueta.textContent = "Em breve";

        const titulo = document.createElement("h3");
        titulo.textContent = "Novidades do universo RPG";

        const texto = document.createElement("p");
        texto.textContent =
            "Os destaques da comunidade aparecerão aqui assim que houver conteúdos publicados.";

        conteudo.append(etiqueta, titulo, texto);
        vazio.appendChild(conteudo);
        container.appendChild(vazio);
        return;
    }

    const destaque = destaques[indiceDestaque];
    const slide = document.createElement("article");
    slide.className = "featured-slide";

    const imagemUrl = obter(destaque, "imagemUrl", "ImagemUrl");

    if (imagemUrl) {
        const urlSegura = validarUrl(imagemUrl);

        if (urlSegura) {
            const imagem = document.createElement("img");
            imagem.className = "featured-slide-image";
            imagem.src = urlSegura;
            imagem.alt = "";
            imagem.loading = "lazy";

            imagem.addEventListener("error", () => imagem.remove());
            slide.classList.add("has-image");
            slide.appendChild(imagem);
        }
    }

    const conteudo = document.createElement("div");
    conteudo.className = "featured-content";

    const etiqueta = document.createElement("span");
    etiqueta.className = "featured-tag";
    etiqueta.textContent =
        obter(destaque, "tipo", "Tipo") || "Destaque";

    const titulo = document.createElement("h3");
    titulo.textContent =
        obter(destaque, "titulo", "Titulo") || "Destaque do Fichality";

    const resumo = document.createElement("p");
    resumo.textContent =
        obter(destaque, "resumo", "Resumo") || "";

    conteudo.append(etiqueta, titulo, resumo);

    const linkInformado = obter(destaque, "linkUrl", "LinkUrl");
    const linkSeguro = linkInformado ? validarUrl(linkInformado) : null;

    if (linkSeguro) {
        const link = document.createElement("a");
        link.className = "featured-link";
        link.href = linkSeguro;
        link.textContent = "Conhecer destaque →";

        // Links externos são abertos em outra aba.
        if (new URL(linkSeguro, window.location.origin).origin
            !== window.location.origin) {
            link.target = "_blank";
            link.rel = "noopener noreferrer";
        }

        conteudo.appendChild(link);
    }

    slide.appendChild(conteudo);
    container.appendChild(slide);

    destaques.forEach((_, indice) => {
        const botao = document.createElement("button");
        botao.type = "button";
        botao.className =
            indice === indiceDestaque ? "active" : "";

        botao.setAttribute(
            "aria-label",
            `Exibir destaque ${indice + 1}`
        );

        botao.setAttribute(
            "aria-current",
            indice === indiceDestaque ? "true" : "false"
        );

        botao.addEventListener("click", () => {
            indiceDestaque = indice;
            renderizarDestaque();
        });

        indicadores.appendChild(botao);
    });
}

function obter(objeto, camelCase, pascalCase) {
    return objeto?.[camelCase] ?? objeto?.[pascalCase] ?? "";
}

function validarUrl(valor) {
    try {
        const url = new URL(valor, window.location.origin);

        if (url.protocol !== "https:" && url.protocol !== "http:") {
            return null;
        }

        return url.href;
    } catch {
        return null;
    }
}

// --------------------------------------------------------------------------
// Criar uma ficha
// --------------------------------------------------------------------------

function abrirDialogNovaFicha() {
    document.getElementById("erroNovaFicha").textContent = "";
    document.getElementById("dialogNovaFicha").showModal();
    document.getElementById("novoNomePersonagem").focus();
}

function fecharDialogNovaFicha() {
    document.getElementById("dialogNovaFicha").close();
}

async function criarNovaFicha(evento) {
    evento.preventDefault();

    const form = evento.currentTarget;
    const botao = document.getElementById("confirmarNovaFicha");
    const erro = document.getElementById("erroNovaFicha");

    erro.textContent = "";

    if (!usuarioAtual?.id) {
        erro.textContent =
            "Não foi possível identificar sua sessão. Entre novamente.";
        return;
    }

    const nomePersonagem =
        document.getElementById("novoNomePersonagem").value.trim();

    const sistemaRpg =
        document.getElementById("novoSistemaRpg").value.trim();

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
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                nomePersonagem,
                sistemaRpg,
                usuarioId: usuarioAtual.id
            })
        });

        const textoResposta = await resposta.text();
        let resultado = {};

        try {
            resultado = textoResposta ? JSON.parse(textoResposta) : {};
        } catch {
            resultado = {};
        }

        if (!resposta.ok) {
            throw new Error(
                resultado.mensagem ||
                resultado.title ||
                `Não foi possível criar a ficha (HTTP ${resposta.status}).`
            );
        }

        if (!resultado.fichaId) {
            throw new Error(
                "A API respondeu, mas não retornou o ID da nova ficha."
            );
        }

        window.location.href =
            `home.html?id=${encodeURIComponent(resultado.fichaId)}`;
    } catch (erroCriacao) {
        console.error("Erro ao criar ficha:", erroCriacao);
        erro.textContent =
            erroCriacao.message || "Erro ao criar a ficha.";
    } finally {
        botao.disabled = false;
        botao.textContent = "Criar ficha";
    }
}
