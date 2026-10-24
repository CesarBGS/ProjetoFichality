// Estado global da ficha no Front-End
let fichaAtual = {
    id: null,
    nomePersonagem: "",
    sistemaRpg: "",
    blocos: []
};

// 1. Carregar ficha existente do Banco via API (GET)
async function carregarFichaPorId(id) {
    try {
        const response = await fetch(`/api/ficha/${id}`);
        if (!response.ok) throw new Error("Ficha não encontrada");

        const data = await response.json();
        fichaAtual.id = data.id;
        fichaAtual.nomePersonagem = data.nomePersonagem;
        fichaAtual.sistemaRpg = data.sistemaRpg;

        // Converte a string JSON armazenada na coluna NVARCHAR(MAX) em objeto JS
        fichaAtual.blocos = JSON.parse(data.estruturaJson || "[]");

        document.getElementById('nomePersonagem').value = fichaAtual.nomePersonagem;
        document.getElementById('sistemaRpg').value = fichaAtual.sistemaRpg;

        renderizarEditor();
    } catch (err) {
        console.error("Erro ao carregar ficha:", err);
    }
}

// 2. Renderizar a árvore de blocos e campos no DOM
function renderizarEditor() {
    const container = document.getElementById('blocosContainer');
    if (!container) return;

    container.innerHTML = '';

    fichaAtual.blocos.forEach((bloco, bIndex) => {
        const blocoEl = document.createElement('div');
        blocoEl.className = 'bloco-card';
        blocoEl.innerHTML = `
            <div class="bloco-header">
                <input type="text" class="input-titulo" value="${bloco.titulo || ''}" 
                    onchange="atualizarTituloBloco(${bIndex}, this.value)" placeholder="Título do Bloco">
                <div class="bloco-acoes">
                    <button type="button" onclick="moverBloco(${bIndex}, -1)">▲</button>
                    <button type="button" onclick="moverBloco(${bIndex}, 1)">▼</button>
                    <button type="button" class="btn-del" onclick="removerBloco(${bIndex})">Excluir</button>
                </div>
            </div>
            <div class="campos-container">
                ${(bloco.campos || []).map((campo, cIndex) => `
                    <div class="campo-linha">
                        <input type="text" value="${campo.chave || ''}" 
                            onchange="atualizarCampo(${bIndex}, ${cIndex}, 'chave', this.value)" placeholder="Nome (ex: Força)">
                        <input type="text" value="${campo.valor || ''}" 
                            onchange="atualizarCampo(${bIndex}, ${cIndex}, 'valor', this.value)" placeholder="Valor (ex: 18)">
                        <button type="button" class="btn-del-sm" onclick="removerCampo(${bIndex}, ${cIndex})">×</button>
                    </div>
                `).join('')}
            </div>
            <button type="button" class="btn-add-sm" onclick="adicionarCampo(${bIndex})">+ Adicionar Campo</button>
        `;
        container.appendChild(blocoEl);
    });
}

// 3. Manipulação de Blocos e Campos em Memória
function adicionarBloco() {
    fichaAtual.blocos.push({
        titulo: "Novo Bloco",
        campos: [{ chave: "Campo", valor: "Valor" }]
    });
    renderizarEditor();
}

function removerBloco(index) {
    fichaAtual.blocos.splice(index, 1);
    renderizarEditor();
}

function moverBloco(index, direcao) {
    const novoIndex = index + direcao;
    if (novoIndex < 0 || novoIndex >= fichaAtual.blocos.length) return;
    const temp = fichaAtual.blocos[index];
    fichaAtual.blocos[index] = fichaAtual.blocos[novoIndex];
    fichaAtual.blocos[novoIndex] = temp;
    renderizarEditor();
}

function atualizarTituloBloco(index, valor) {
    fichaAtual.blocos[index].titulo = valor;
}

function adicionarCampo(bIndex) {
    if (!fichaAtual.blocos[bIndex].campos) fichaAtual.blocos[bIndex].campos = [];
    fichaAtual.blocos[bIndex].campos.push({ chave: "", valor: "" });
    renderizarEditor();
}

function atualizarCampo(bIndex, cIndex, prop, valor) {
    fichaAtual.blocos[bIndex].campos[cIndex][prop] = valor;
}

function removerCampo(bIndex, cIndex) {
    fichaAtual.blocos[bIndex].campos.splice(cIndex, 1);
    renderizarEditor();
}

// 4. Salvar a estrutura serializada em JSON no SQL Server (PUT)
async function salvarEstruturaFicha() {
    if (!fichaAtual.id) {
        alert("Carregue ou crie uma ficha antes de salvar.");
        return;
    }

    const payload = {
        fichaId: fichaAtual.id,
        estruturaJson: JSON.stringify(fichaAtual.blocos)
    };

    try {
        const response = await fetch('/api/ficha/salvar-estrutura', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        const resultado = await response.json();
        if (response.ok) {
            alert("Estrutura da ficha salva no SQL Server com sucesso!");
        } else {
            alert("Erro ao salvar: " + (resultado.mensagem || "Erro na requisição."));
        }
    } catch (err) {
        console.error("Erro ao conectar com a API:", err);
    }
}