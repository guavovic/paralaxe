# Guia de produção de arte

Como preparar as camadas de um cenário para o Paralaxe, desenhando à mão, em pixel art, com IA ou misturando as três coisas. Vale para qualquer jogo que use o pacote. Os números do exemplo da floresta aparecem como referência, porque já estão testados.

A regra que atravessa o guia inteiro: **a arte entrega a forma, o pacote entrega o movimento e a profundidade.** Desenhe nítido e parado. Desfoque, vento, névoa que respira, brilho que pulsa e rolagem ficam por conta do pacote, e assim dá para ajustar tudo sem redesenhar.

## 1. Antes de desenhar: três decisões

### Modo

| Arte | Modo | Câmera | Filtro na Unity |
|---|---|---|---|
| Pixel art | 2D | ortográfica | Point |
| Pintada, vetor, à mão | 2D ou perspectiva | ortográfica ou perspectiva | Bilinear |

Em pixel art, use o modo 2D: todas as camadas ficam na mesma grade e os pixels têm o mesmo tamanho na tela inteira. Em perspectiva, as camadas distantes são ampliadas e os pixels delas ficam maiores que os do primeiro plano, o que só funciona com arte pintada.

### Resolução base

É o tamanho da tela do jogo em pixels de arte. Tudo sai dela.

| Resolução base | Escala até 1920x1080 | Indicada para |
|---|---|---|
| 320x180 | 6x | pixel art bem grossa, estilo 8 e 16 bits |
| 480x270 | 4x | pixel art com detalhe (é a do exemplo) |
| 640x360 | 3x | pixel art fina, ou base para arte pintada |

Arte pintada não tem grade, mas a resolução base continua servindo de referência de proporção. Pinte em 2x ou 4x a resolução da tela final e exporte no tamanho em que ela vai aparecer.

### Pixels por unidade (PPU)

Com câmera ortográfica, a tela tem `2 x Orthographic Size` unidades de altura. Para uma camada da altura da tela ocupar a tela exata:

```
PPU = altura da resolução base / (2 x Orthographic Size)
```

No exemplo: 270 / (2 x 5) = **27 PPU**, com Orthographic Size 5.

**Use o mesmo PPU em todas as camadas e nunca mude a escala do objeto da camada.** Escala diferente quebra a grade: uma camada fica com pixel maior que a outra. Se uma camada precisa ficar maior, desenhe ela maior.

## 2. Tamanho de cada camada

### Largura

O pacote cria uma cópia de cada lado da imagem (esquerda e direita) e vai trocando de lugar conforme a câmera anda. São três blocos lado a lado. Então:

- **mínimo:** a largura da tela (480 px na base 480x270). Mais estreito que isso, aparece buraco na borda.
- **recomendado:** de 1x a 2x a largura da tela. Quanto mais larga, menos o jogador percebe a repetição.
- **camadas muito distantes** (céu, montanhas) quase não andam e podem ficar no mínimo.
- **camadas próximas** (chão, primeiro plano) passam rápido e mostram a repetição primeiro. Ou desenhe mais largo, ou deixe a imagem simples e tire a variação dos [elementos espalhados](#5-o-que-o-pacote-faz-por-voce).

A largura do loop é a largura do **primeiro filho** da camada. Se uma camada tem várias imagens, a primeira é que manda.

### Altura

O jeito mais simples, e o do exemplo, é **toda camada ter a altura da tela**, com transparente onde não há nada. Assim todas usam o mesmo pivô central e se alinham sozinhas.

Se a câmera sobe e desce (pulo alto, fase vertical), as camadas do meio precisam de folga:

- camadas distantes (fator 0,7 ou mais) acompanham a câmera na vertical e nunca mostram a borda. Não precisam de folga.
- camadas do meio acompanham só uma parte. Com fator vertical 0,5 e a câmera subindo 4 unidades, a camada sobe 2 e mostra 2 unidades de borda. Desenhe essa sobra em cima e embaixo (2 x 27 = 54 px no exemplo).
- o chão (fator 0) fica parado no mundo, como o resto da fase.

Na dúvida, adicione 25% de altura em cima e embaixo das camadas do meio.

### Grade de pixel e escala por profundidade

Pixel art com profundidade engana o olho se o objeto distante tiver o mesmo detalhe que o próximo. Não reduza a escala: **reduza o detalhe**.

| Profundidade | Detalhe | Contorno | Cores por objeto |
|---|---|---|---|
| Longe | silhueta, quase sem textura | nenhum | 1 ou 2 |
| Meio | formas principais, pouca textura | só luz de contorno | 3 ou 4 |
| Perto (chão) | detalhe completo | sim, se o jogo usa | o que precisar |
| Primeiro plano | silhueta escura, desfocada pelo pacote | nenhum | 1 ou 2 |

Um pinheiro distante tem 20 px e três tons; o mesmo pinheiro no meio tem 60 px e textura de agulha. Os dois estão na mesma grade.

## 3. Loop sem emenda

A borda esquerda e a direita da imagem têm que se encaixar. Como fazer:

1. Desenhe a camada inteira sem pensar na emenda.
2. Desloque a imagem metade da largura na horizontal, com o conteúdo que sai de um lado voltando pelo outro. No Aseprite é **Edit > Shift** (ou o modo Tiled na horizontal); no Photoshop, **Filtro > Outros > Deslocamento** com "Envolver"; no Krita, **Camada > Deslocar** com "Wrap around".
3. A emenda agora está no meio da imagem, visível. Redesenhe essa faixa até sumir.
4. Desfaça o deslocamento (ou deixe assim, o loop não liga de onde começa).

**Para testar:** ponha a imagem duas vezes lado a lado e olhe a junção. Ou, mais rápido, arraste para o Paralaxe e anime o preview: a emenda aparece toda vez que a câmera passa por ela.

O que denuncia a emenda, mesmo com as bordas encaixando:

- um objeto marcante (árvore torta, pedra grande) que volta sempre igual. Tire da imagem e use como elemento espalhado.
- o horizonte em alturas diferentes nas duas pontas.
- gradiente de luz da esquerda para a direita. A luz de uma camada em loop tem que ser igual nas duas pontas.

Névoa e brilho com o shader do pacote também respeitam o loop: o ruído da névoa repete na mesma largura da camada.

## 4. Quantas camadas e como separar

De 6 a 9 camadas cobrem quase todo cenário. O exemplo tem 15 a 18 por bioma, porque mostra todos os efeitos de uma vez; não precisa tanto.

| Camada | Fator 2D | O que entra | Observação |
|---|---|---|---|
| Céu | 0,99 | gradiente, estrelas, lua, sol | quase parado. Pode ser só um degradê |
| Nuvens | 0,96 | nuvens soltas | rolagem automática lenta, meio transparente |
| Distante | 0,9 a 0,93 | montanhas, prédios ao longe, silhuetas | cor puxada para o céu, pouco contraste |
| Neblina distante | 0,85 | faixa de névoa | shader Mist, separa os planos |
| Fundo | 0,7 a 0,8 | árvores, prédios, colunas | desfoque leve, um pouco de vento |
| Meio | 0,5 a 0,6 | o que dá cara ao lugar | nítido, cores do bioma |
| Chão | 0 | onde a ação acontece | parado no mundo, com o colisor da fase |
| Primeiro plano | -0,3 a -0,4 | galhos, postes, estalactites | escuro, desfocado, rápido, pouco e esparso |

O editor já distribui as distâncias quando você arrasta as imagens, da mais distante para a mais próxima, e tem presets por bioma (floresta, caverna, pântano, neve, deserto, cidade) com esses valores.

**Como saber se algo vai numa camada própria:** se precisa de movimento diferente (vento, névoa, brilho, rolagem) ou de distância diferente, vai separado. Uma lanterna acesa vira duas camadas: a lanterna, normal, e o halo de luz, com o shader Glow. Névoa nunca vai pintada dentro de outra camada.

**Primeiro plano com moderação.** Ele passa na frente da ação. Pouco, escuro e esparso; se cobrir mais de 15% da tela em algum momento, está demais.

## 5. O que o pacote faz por você

Não pinte o que está nesta lista. Pinte nítido e parado e ajuste na Unity.

| Efeito | Onde fica | O que desenhar | O que não desenhar |
|---|---|---|---|
| **Desfoque** (0 a 6 texels) | campo da camada | a camada nítida | desfoque pintado. Com ele, não dá para ajustar depois |
| **Tint** | campo da camada | cores no tom normal | o escurecimento ou a cor da distância. Ajuste no tint |
| **Rolagem automática** | campo da camada | nuvem, neblina, neve caindo | nada a mais: a camada anda sozinha |
| **Wind** | material Wind Sprite | folhagem, mato | a folhagem já torta pelo vento |
| **Sway** | material Sway Sprite | planta presa no chão, ou cipó e estandarte presos no topo (opção "Hanging") | o objeto já inclinado |
| **Mist** | material Mist Sprite | a **forma** da névoa: densidade em branco e cinza, com alpha | o movimento ou a textura de nuvem. O shader cria o ruído, a respiração e a deriva |
| **Glow** | material Glow Sprite (aditivo) | só a mancha da luz, clara, sobre transparente | a lâmpada junto. O pulsar e o tremular de fogo são do shader |
| **Ripple** | material Ripple Sprite | água, reflexo, ar quente | ondinhas desenhadas |
| **Trechos** | seção Trechos da camada | um bloco por cenário e, se quiser, um bloco de costura | a fase inteira numa imagem só |
| **Elementos espalhados** | seção Espalhados da camada | cada objeto solto, num sprite próprio | os objetos repetidos dentro da imagem do loop |

Notas sobre cada um:

- **Glow** soma luz em cima do que está atrás (blend aditivo). Preto vira invisível. Desenhe a mancha de luz com a borda caindo para o transparente; o pacote cuida do desfoque, da intensidade e do pulsar.
- **Mist** vem com blend normal para névoa e pode virar aditivo (Dst Blend em One) para raios de luz, como no material `MistBeams` do exemplo.
- **Trechos:** cada camada muda de arte a partir de uma posição X da câmera. O bloco novo entra fora da tela, e o bloco de costura (opcional) faz a passagem do cenário velho para o novo, por exemplo floresta virando ruína. A costura tem a mesma largura e a mesma altura do bloco; a borda esquerda dela encaixa no cenário velho e a direita no novo. Camadas muito distantes podem esmaecer inteiras em vez de trocar bloco a bloco ("Esmaecer a camada inteira"), e aí não precisam de costura.
- **Elementos espalhados:** o pacote sorteia posição, altura, tamanho e espelhamento num trecho bem maior que a imagem, com semente fixa, e eles não seguem o loop. É assim que se quebra a repetição. Desenhe de 3 a 6 variações de cada objeto (moitas, pedras, cogumelos, postes), todas com a base na borda de baixo do sprite, e eles podem reagir ao herói (balançar ou dar um pulinho) e ser cortados.
- **Vento por trecho e por grupo:** cada trecho e cada grupo de espalhados tem o seu vento. Pedra e construção ficam com vento 0, mesmo numa camada de mato.

## 6. Transparência e recorte

O que mais estraga camada é a borda.

- **Pixel art: alpha só 0 ou 255.** Nada de meio-transparente na borda. Com filtro Point, borda semitransparente vira um contorno sujo que pisca quando a camada anda.
- **Halo:** é a franja clara ou escura em volta do recorte, que vem da cor que ficava atrás antes de recortar. Acontece com fundo removido por varinha mágica ou por IA. Para tirar: selecione o recorte, contraia 1 px e apague a borda, ou repinte a borda com a cor do objeto.
- **Arte pintada pode ter borda suave**, mas o pixel transparente tem que guardar a cor do objeto, não preto nem branco. Na Unity, ligue **Alpha Is Transparency** na importação, que dilata a cor para dentro do transparente.
- **Névoa, brilho e nuvem** são as exceções: alpha gradual é o material delas. Ainda assim, nada de borda dura onde a imagem termina; o desenho tem que sumir antes da borda da imagem (menos nas bordas do loop, que encaixam).
- **Fundo do céu** é a única camada sem transparente. Todas as outras têm transparente em volta.
- **Sem cor sólida de fundo nos PNGs.** Fundo branco, preto ou magenta "para recortar depois" sempre deixa resto.

## 7. Paleta e contraste com a ação

O fundo existe para a ação ficar legível. Herói, nave, inimigos, tiros e itens são o que o jogador precisa ler em décimos de segundo; o fundo não pode competir.

- **Reserve as cores mais saturadas e mais claras para a ação.** Se os tiros são laranja forte, nenhum laranja forte no fundo.
- **Contraste cai com a distância.** Longe, valores próximos entre si e puxados para a cor do céu (perspectiva atmosférica). Perto, mais contraste. O primeiro plano quebra a regra para o lado escuro: quase silhueta.
- **Teste em preto e branco.** Tire a saturação do print do jogo. Se o herói some no fundo, falta diferença de valor, não de cor.
- **Teste borrado.** Desfoque o print. Se ainda dá para ver onde está o herói e onde estão os inimigos, o contraste está certo.
- **Jogo de nave e de ação rápida:** fundo mais escuro e menos detalhado que num jogo de exploração. A camada do meio, que passa mais rápido, é a que mais atrapalha a leitura dos tiros: deixe ela com menos detalhe ou mais escura.
- **Paleta limitada por bioma** (16 a 32 cores) e uma ou duas cores de destaque. Em pixel art, use uma paleta pronta (Lospec tem centenas) e mantenha a mesma em todas as camadas do bioma.
- **A luz vem do mesmo lado em todas as camadas.** A luz de contorno (a borda iluminada contra o fundo) é o que mais dá cara de profundidade em pixel art; use em camadas do meio e do fundo.

## 8. Exportação e importação na Unity

### Exportar

- **PNG de 32 bits** (RGBA), sem compressão com perda. Nada de JPG.
- **Um arquivo por camada**, com nome que diz a ordem: `floresta_00_ceu.png`, `floresta_01_nuvens.png`, ..., `floresta_12_primeiro_plano.png`. O número já é a ordem de arraste no editor.
- Todas as camadas do mesmo bioma com **o mesmo tamanho de tela** (a menos que tenham folga vertical, aí o mesmo tamanho entre si).
- **Pixel art em 1x**, no tamanho real dos pixels. Ampliar 4x antes de exportar só pesa mais e atrapalha o PPU.
- Bloco de costura com o nome do par: `floresta_para_templo_00.png`.

### Importar

Na janela de importação de cada imagem (selecione todas de uma vez para ajustar juntas):

| Campo | Pixel art | Arte pintada |
|---|---|---|
| Texture Type | Sprite (2D and UI) | Sprite (2D and UI) |
| Sprite Mode | Single | Single |
| Pixels Per Unit | o mesmo em todas (27 no exemplo) | o mesmo em todas |
| Mesh Type | **Full Rect** | **Full Rect** |
| Extrude Edges | 1 | 1 |
| Pivot | Center | Center |
| Alpha Is Transparency | ligado | ligado |
| Generate Mip Maps | desligado | ligado só no modo perspectiva |
| Wrap Mode | Clamp | Clamp |
| Filter Mode | **Point (no filter)** | Bilinear |
| Compression | **None** | Normal ou High Quality |

Por que cada um importa:

- **Full Rect** porque os shaders de vento, balanço e ondulação deslocam os pixels dentro do retângulo do sprite. Com Tight, a malha corta o desenho no contorno e o que balança some na borda.
- **Point e sem compressão** em pixel art porque compressão cria blocos de cor e o Bilinear borra a grade.
- **Pivot no centro** porque o pacote posiciona a camada pelo centro.
- **Elementos espalhados:** o pivô é o ponto que fica na altura sorteada. Com **Bottom**, a base de todas as variações apoia na mesma linha e o tamanho sorteado cresce para cima, sem afundar no chão. O exemplo usa Center, com sprites da mesma altura; com variações de alturas diferentes, Bottom dá menos trabalho.

### Ordem de desenho (sorting)

Ao arrastar as imagens para o editor, o pacote dá a ordem de desenho de trás para a frente sozinho. Os elementos espalhados têm o próprio Sorting Order na seção deles. O herói, os inimigos e os tiros ficam numa ordem entre a camada do chão e a do primeiro plano.

## 9. Usando IA

IA gera boas ideias e boas bases, mas **não entrega uma camada pronta**: o resultado não tem transparência de verdade, não fecha o loop e não respeita a grade de pixel. Conte com limpeza em todas as imagens.

### O fluxo que funciona

1. **Gere a cena inteira primeiro**, como referência de clima, paleta e composição. Ela não vai para o jogo.
2. **Gere cada camada separada**, em cima de fundo de cor sólida, com o prompt dizendo o que é aquela camada. Uma camada por pedido.
3. **Remova o fundo** (ferramenta de remoção de fundo, ou varinha mágica no fundo sólido) e limpe o halo (seção 6).
4. **Reduza para a resolução base** com vizinho mais próximo (nearest neighbor), nunca bilinear. Imagem de IA "em pixel art" quase nunca está numa grade real; a redução é o que cria a grade.
5. **Reduza a paleta** para a paleta do bioma (Aseprite: Sprite > Color Mode > Indexed, com a paleta carregada).
6. **Feche o loop** como na seção 3: deslocar meia largura e redesenhar a emenda. A IA não faz isso sozinha.
7. **Limpe à mão:** pixels soltos, contorno quebrado, borda semitransparente, objeto marcante que denuncia a repetição.

### Prompts

Escrever em inglês costuma dar resultado melhor. Um modelo para cada camada:

```
2D side-scrolling game background layer, [o que é a camada: distant mountain silhouettes],
pixel art, [16-bit / SNES era], flat orthographic side view, no perspective,
[paleta: muted blue and purple, low contrast], horizontal composition,
isolated on solid flat [magenta #FF00FF] background, no ground, no sky,
no characters, no text, no frame, no shadows on the background,
sharp edges, no anti-aliasing, no gradients on the edges
```

Para o céu, que é a única camada sem transparente:

```
2D game sky background, [night sky with stars and a large moon],
pixel art, [16-bit], smooth vertical gradient, flat side view,
no clouds, no mountains, no horizon objects, no text
```

Para elementos espalhados:

```
pixel art sprite sheet of [5 different small bushes], side view, same scale,
each one standing on the same baseline, evenly spaced,
on solid flat [green #00FF00] background, no shadows, no ground,
no anti-aliasing, [paleta do bioma]
```

Dicas:

- **Escolha a cor do fundo sólido longe das cores da camada.** Magenta para florestas e cavernas; verde para cenários roxos e vermelhos.
- **Repita as mesmas palavras de estilo e paleta em todas as camadas** do bioma. Mudar uma palavra muda o estilo.
- **"Flat side view, no perspective"** é o que evita o chão em perspectiva, que não funciona em parallax lateral.
- **Peça largo:** proporção 3:1 ou 4:1 quando o gerador permitir. Depois corte na largura do loop.
- **Use a cena de referência como imagem de entrada** (image to image) ao gerar cada camada, quando o gerador permitir. Ajuda a manter paleta e luz iguais.
- **Névoa e brilho não precisam de IA:** um pincel macio em branco sobre transparente, com o shader Mist ou Glow, fica melhor e leva dois minutos.
- **Confira os termos de uso** do gerador antes de pôr a arte em jogo comercial.

## 10. Checklist de um cenário novo

- [ ] Modo escolhido (2D para pixel art, perspectiva só para arte pintada)
- [ ] Resolução base, Orthographic Size e PPU definidos e anotados
- [ ] Lista de camadas com o que entra em cada uma (seção 4)
- [ ] Paleta do bioma escolhida, com as cores da ação de fora
- [ ] Cada camada desenhada nítida, sem desfoque, vento ou névoa pintados
- [ ] Largura de cada camada de pelo menos a largura da tela
- [ ] Loop testado lado a lado em todas as camadas
- [ ] Folga vertical nas camadas do meio, se a câmera sobe e desce
- [ ] Borda sem meio-transparente (pixel art) e sem halo
- [ ] Objetos marcantes tirados do loop e exportados como elementos espalhados, com pivô na base
- [ ] Luz e brilho em camadas separadas, para o Glow
- [ ] PNGs numerados de trás para a frente
- [ ] Importação conferida (PPU, Full Rect, Point e sem compressão em pixel art)
- [ ] Arrastadas para o editor na ordem, com um preset de bioma como ponto de partida
- [ ] Teste em preto e branco e borrado com a ação na tela
