
/* ==========================================================================
   FICHALITY — GUARDA DE NAVEGAÇÃO
   A sessão real é validada pela API e pelo cookie HttpOnly.
   ========================================================================== */

(function verificarSessao() {
    const path = window.location.pathname.toLowerCase();

    const isLandingPage =
        path.endsWith("index.html") || path === "/";

    const isHomePage = path.endsWith("home.html");
    const isEditorPage = path.endsWith("editor.html");

    fetch("/api/autenticacao/me", {
        credentials: "same-origin"
    })
        .then(async response => {
            if (response.ok) {
                const usuario = await response.json();

                localStorage.setItem(
                    "fichality_user",
                    JSON.stringify(usuario)
                );

                // Usuário autenticado não precisa voltar ao login.
                if (isLandingPage) {
                    window.location.replace("home.html");
                }

                return;
            }

            localStorage.removeItem("fichality_user");

            if (isHomePage || isEditorPage) {
                window.location.replace("index.html");
            }
        })
        .catch(erro => {
            console.error("Falha ao verificar a sessão:", erro);

            if (isHomePage || isEditorPage) {
                window.location.replace("index.html");
            }
        });
})();


document.addEventListener('DOMContentLoaded', () => {
    const formLogin = document.getElementById('formLogin');
    const formRegistro = document.getElementById('formRegistro');
    const alertLogin = document.getElementById('alertLogin');
    const alertRegistro = document.getElementById('alertRegistro');
    const btnSair = document.getElementById('btnSair');

    function exibirErro(elementoAlert, mensagem) {
        if (!elementoAlert) {
            console.error(mensagem);
            return;
        }
        elementoAlert.textContent = mensagem;
        elementoAlert.style.display = 'block';
    }

    function limparAlertas() {
        if (alertLogin) alertLogin.style.display = 'none';
        if (alertRegistro) alertRegistro.style.display = 'none';
    }

    async function lerResposta(response) {
        const text = await response.text();
        if (!text) return {};
        try {
            return JSON.parse(text);
        } catch {
            return { mensagem: text };
        }
    }

    function mensagemDoErro(data) {
        if (data?.mensagem) return data.mensagem;
        if (data?.detail) return data.detail;
        if (data?.title) return data.title;
        if (data?.errors && typeof data.errors === 'object') {
            return Object.values(data.errors).flat().join(' ');
        }
        return 'Não foi possível concluir a operação.';
    }

    async function enviarAutenticacao(url, payload, alertElement) {
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'same-origin',
            body: JSON.stringify(payload)
        });
        const data = await lerResposta(response);

        if (!response.ok) {
            console.error('Falha de autenticação:', response.status, data);
            exibirErro(alertElement, `Erro ${response.status}: ${mensagemDoErro(data)}`);
            return false;
        }

        localStorage.setItem('fichality_user', JSON.stringify(data));
        window.location.assign('/home.html');
        return true;
    }

    if (formLogin) {
        formLogin.addEventListener('submit', async (event) => {
            event.preventDefault();
            limparAlertas();

            const email = document.getElementById('loginEmail')?.value.trim();
            const senha = document.getElementById('loginSenha')?.value;
            if (!email || !senha) {
                exibirErro(alertLogin, 'Preencha o e-mail e a senha.');
                return;
            }

            try {
                await enviarAutenticacao('/api/autenticacao/login', { email, senha }, alertLogin);
            } catch (error) {
                console.error('Erro de requisição no login:', error);
                exibirErro(alertLogin, 'Falha ao conectar com a API. Verifique se o projeto está em execução.');
            }
        });
    }

    if (formRegistro) {
        formRegistro.addEventListener('submit', async (event) => {
            event.preventDefault();
            limparAlertas();

            const nome = document.getElementById('regNome')?.value.trim();
            const email = document.getElementById('regEmail')?.value.trim();
            const senha = document.getElementById('regSenha')?.value;

            if (!nome || !email || !senha) {
                exibirErro(alertRegistro, 'Preencha todos os campos obrigatórios.');
                return;
            }
            if (senha.length < 8) {
                exibirErro(alertRegistro, 'A senha deve ter pelo menos 8 caracteres.');
                return;
            }

            try {
                await enviarAutenticacao('/api/autenticacao/registrar', { nome, email, senha }, alertRegistro);
            } catch (error) {
                console.error('Erro de requisição no cadastro:', error);
                exibirErro(alertRegistro, 'Falha ao conectar com a API. Verifique se o projeto está em execução.');
            }
        });
    }

    if (btnSair) {
        btnSair.addEventListener("click", async event => {
            event.preventDefault();

            try {
                await fetch("/api/autenticacao/logout", {
                    method: "POST",
                    credentials: "same-origin"
                });
            } catch (erro) {
                console.error("Erro ao encerrar sessão:", erro);
            } finally {
                localStorage.removeItem("fichality_user");
                window.location.replace("index.html");
            }
        });
    }
});

// Estado atual da ficha no frontend.
let fichaAtual = {
    id: null,
    nomePersonagem: '',
    sistemaRpg: '',
    blocos: []
};

async function lerJsonDaResposta(response) {
    const text = await response.text();
    if (!text) return {};
    try {
        return JSON.parse(text);
    } catch {
        return { mensagem: text };
    }
}

function mensagemApi(data, fallback) {
    return data?.mensagem || data?.title || data?.detail || fallback;
}

async function carregarFichaPorId(id) {
    try {
        const response = await fetch(`/api/ficha/${encodeURIComponent(id)}`, {
            credentials: 'same-origin'
        });
        const data = await lerJsonDaResposta(response);

        if (!response.ok) throw new Error(mensagemApi(data, `Erro ${response.status} ao carregar a ficha.`));

        fichaAtual.id = data.id;
        fichaAtual.nomePersonagem = data.nomePersonagem || '';
        fichaAtual.sistemaRpg = data.sistemaRpg || '';

        try {
            const parsed = JSON.parse(data.estruturaJson || '[]');
            fichaAtual.blocos = Array.isArray(parsed) ? parsed : [];
        } catch {
            throw new Error('A estrutura JSON desta ficha está inválida.');
        }

        const nomeInput = document.getElementById('nomePersonagem');
        const sistemaInput = document.getElementById('sistemaRpg');
        if (nomeInput) nomeInput.value = fichaAtual.nomePersonagem;
        if (sistemaInput) sistemaInput.value = fichaAtual.sistemaRpg;

        renderizarEditor();
        return true;
    } catch (error) {
        console.error('Erro ao carregar ficha:', error);
        alert(error.message || 'Não foi possível carregar a ficha.');
        return false;
    }
}

// Criar ficha pela API. Chame esta função a partir do formulário/botão de criação da interface.
async function criarFicha(nomePersonagem, sistemaRpg) {
    try {
        const response = await fetch('/api/ficha/criar', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'same-origin',
            body: JSON.stringify({ nomePersonagem, sistemaRpg })
        });
        const data = await lerJsonDaResposta(response);

        if (!response.ok) throw new Error(mensagemApi(data, `Erro ${response.status} ao criar ficha.`));
        await carregarFichaPorId(data.fichaId);
        return data.fichaId;
    } catch (error) {
        console.error('Erro ao criar ficha:', error);
        alert(error.message || 'Não foi possível criar a ficha.');
        return null;
    }
}

function criarBotao(texto, classe, acao) {
    const button = document.createElement('button');
    button.type = 'button';
    button.textContent = texto;
    if (classe) button.className = classe;
    button.addEventListener('click', acao);
    return button;
}

function renderizarEditor() {
    const container = document.getElementById('blocosContainer');
    if (!container) return;

    container.replaceChildren();

    fichaAtual.blocos.forEach((bloco, bIndex) => {
        const blocoEl = document.createElement('div');
        blocoEl.className = 'bloco-card';

        const header = document.createElement('div');
        header.className = 'bloco-header';

        const titulo = document.createElement('input');
        titulo.type = 'text';
        titulo.className = 'input-titulo';
        titulo.placeholder = 'Título do Bloco';
        titulo.value = bloco.titulo || '';
        titulo.addEventListener('change', () => atualizarTituloBloco(bIndex, titulo.value));
        header.appendChild(titulo);

        const acoes = document.createElement('div');
        acoes.className = 'bloco-acoes';
        acoes.appendChild(criarBotao('▲', '', () => moverBloco(bIndex, -1)));
        acoes.appendChild(criarBotao('▼', '', () => moverBloco(bIndex, 1)));
        acoes.appendChild(criarBotao('Excluir', 'btn-del', () => removerBloco(bIndex)));
        header.appendChild(acoes);
        blocoEl.appendChild(header);

        const camposContainer = document.createElement('div');
        camposContainer.className = 'campos-container';

        (Array.isArray(bloco.campos) ? bloco.campos : []).forEach((campo, cIndex) => {
            const linha = document.createElement('div');
            linha.className = 'campo-linha';

            const chave = document.createElement('input');
            chave.type = 'text';
            chave.placeholder = 'Nome (ex: Força)';
            chave.value = campo.chave || '';
            chave.addEventListener('change', () => atualizarCampo(bIndex, cIndex, 'chave', chave.value));

            const valor = document.createElement('input');
            valor.type = 'text';
            valor.placeholder = 'Valor (ex: 18)';
            valor.value = campo.valor || '';
            valor.addEventListener('change', () => atualizarCampo(bIndex, cIndex, 'valor', valor.value));

            linha.append(chave, valor, criarBotao('×', 'btn-del-sm', () => removerCampo(bIndex, cIndex)));
            camposContainer.appendChild(linha);
        });

        blocoEl.appendChild(camposContainer);
        blocoEl.appendChild(criarBotao('+ Adicionar Campo', 'btn-add-sm', () => adicionarCampo(bIndex)));
        container.appendChild(blocoEl);
    });
}

function adicionarBloco() {
    fichaAtual.blocos.push({ titulo: 'Novo Bloco', campos: [{ chave: 'Campo', valor: 'Valor' }] });
    renderizarEditor();
}

function removerBloco(index) {
    fichaAtual.blocos.splice(index, 1);
    renderizarEditor();
}

function moverBloco(index, direcao) {
    const novoIndex = index + direcao;
    if (novoIndex < 0 || novoIndex >= fichaAtual.blocos.length) return;
    [fichaAtual.blocos[index], fichaAtual.blocos[novoIndex]] =
        [fichaAtual.blocos[novoIndex], fichaAtual.blocos[index]];
    renderizarEditor();
}

function atualizarTituloBloco(index, valor) {
    if (fichaAtual.blocos[index]) fichaAtual.blocos[index].titulo = valor;
}

function adicionarCampo(bIndex) {
    const bloco = fichaAtual.blocos[bIndex];
    if (!bloco) return;
    if (!Array.isArray(bloco.campos)) bloco.campos = [];
    bloco.campos.push({ chave: '', valor: '' });
    renderizarEditor();
}

function atualizarCampo(bIndex, cIndex, propriedade, valor) {
    const campo = fichaAtual.blocos[bIndex]?.campos?.[cIndex];
    if (!campo || !['chave', 'valor'].includes(propriedade)) return;
    campo[propriedade] = valor;
}

function removerCampo(bIndex, cIndex) {
    const campos = fichaAtual.blocos[bIndex]?.campos;
    if (!Array.isArray(campos)) return;
    campos.splice(cIndex, 1);
    renderizarEditor();
}

async function salvarEstruturaFicha() {
    if (!fichaAtual.id) {
        alert('Carregue ou crie uma ficha antes de salvar.');
        return;
    }

    try {
        const response = await fetch('/api/ficha/salvar-estrutura', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'same-origin',
            body: JSON.stringify({
                fichaId: fichaAtual.id,
                estruturaJson: JSON.stringify(fichaAtual.blocos)
            })
        });
        const data = await lerJsonDaResposta(response);

        if (response.ok) {
            alert(data.mensagem || 'Estrutura da ficha salva com sucesso!');
        } else {
            alert(`Erro ${response.status}: ${mensagemApi(data, 'Não foi possível salvar a ficha.')}`);
        }
    } catch (error) {
        console.error('Erro ao salvar a estrutura:', error);
        alert('Falha ao conectar com a API. Verifique se o projeto está em execução.');
    }
}
