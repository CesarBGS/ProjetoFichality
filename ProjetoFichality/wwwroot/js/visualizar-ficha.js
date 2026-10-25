document.addEventListener("DOMContentLoaded", carregarFicha);
let fichaId = 0;
async function carregarFicha(){
 const params=new URLSearchParams(location.search); fichaId=Number(params.get("id"));
 const erro=document.getElementById("viewError");
 if(!Number.isInteger(fichaId)||fichaId<=0){erro.textContent="ID da ficha inválido.";return;}
 document.getElementById("btnEditar").href=`/editor.html?id=${encodeURIComponent(fichaId)}`;
 document.getElementById("btnExcluir").addEventListener("click", excluirFicha);
 try{
  const response=await fetch(`/api/ficha/${fichaId}`,{credentials:"same-origin"});
  if(response.status===401){location.replace("/index.html");return;}
  if(!response.ok)throw new Error(response.status===404?"Ficha não encontrada ou sem permissão.":`Falha ao carregar (HTTP ${response.status}).`);
  const ficha=await response.json();
  document.getElementById("nomePersonagem").textContent=ficha.nomePersonagem||"Personagem sem nome";
  document.getElementById("sistemaRpg").textContent=ficha.sistemaRpg||"Sistema não informado";
  let blocos=[];try{blocos=JSON.parse(ficha.estruturaJson||"[]");if(!Array.isArray(blocos))blocos=[];}catch{throw new Error("A estrutura JSON da ficha está inválida.");}
  renderizar(blocos);
 }catch(e){console.error(e);erro.textContent=e.message;document.getElementById("viewContent").replaceChildren();}
}
function renderizar(blocos){
 const root=document.getElementById("viewContent");root.replaceChildren();
 if(!blocos.length){const empty=document.createElement("div");empty.className="view-empty";empty.textContent="Esta ficha ainda não possui blocos. Clique em Editar ficha para começar.";root.appendChild(empty);return;}
 for(const bloco of blocos){
  const section=document.createElement("section");section.className="view-block";
  const title=document.createElement("h2");title.textContent=bloco?.titulo||"Bloco sem título";section.appendChild(title);
  const grid=document.createElement("dl");grid.className="view-fields";
  for(const campo of (Array.isArray(bloco?.campos)?bloco.campos:[])){
   const cell=document.createElement("div");cell.className="view-field";
   const dt=document.createElement("dt");dt.textContent=campo?.chave||"Campo";
   const dd=document.createElement("dd");dd.textContent=(campo?.valor??"").toString()||"—";
   cell.append(dt,dd);grid.appendChild(cell);
  }
  if(!bloco?.campos?.length){const note=document.createElement("p");note.className="view-empty";note.textContent="Este bloco não possui campos.";grid.appendChild(note);}
  section.appendChild(grid);root.appendChild(section);
 }
}
async function excluirFicha(){
 if(!confirm("Deseja excluir esta ficha permanentemente? Essa ação não pode ser desfeita."))return;
 const erro=document.getElementById("viewError");
 try{const response=await fetch(`/api/ficha/${fichaId}`,{method:"DELETE",credentials:"same-origin"});
  if(response.status===401){location.replace("/index.html");return;}
  if(!response.ok){const body=await response.text();throw new Error(body||`Falha ao excluir (HTTP ${response.status}).`);}
  location.replace("/home.html");
 }catch(e){erro.textContent=e.message;}
}
