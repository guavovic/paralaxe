# Paralaxe

![Herói andando pela floresta em pixel art, com as camadas do parallax se movendo em velocidades diferentes](Docs/floresta.gif)

Pacote para a Unity que monta cenários com parallax em camadas, direto no editor, sem escrever código para cada camada. Serve para jogos 2D, em dois modos: um parallax simulado, com um fator de movimento por camada, e um em perspectiva, em que cada camada tem uma profundidade de verdade.

Veio de uma ferramenta de editor simples e virou um pacote, com um cenário de exemplo em pixel art que mostra o que dá para fazer.

## Como foi feito

- **Dois modos de cálculo**, simulado em 2D e em perspectiva, que trocam de um para o outro no mesmo perfil, com a profundidade e o fator equivalentes calculados um a partir do outro.
- **Valores globais** de velocidade e de vento, que o jogador pode afetar e que também afetam o jogador.
- **Efeitos por camada**: desfoque, brilho aditivo, rolagem automática, influência do vento e repetição horizontal sem emenda.
- **Editor próprio**, em `Window > Parallax`, para criar o parallax a partir de imagens, ordenar as camadas e ver o resultado na hora, sem entrar em Play.
- **Cenário de exemplo** em pixel art, com herói animado, câmera que acompanha na horizontal e na vertical, pássaros, vagalumes, luz e neblina. A arte é gerada por script, o que permite refazer o cenário com outra paleta ou outro bioma.
- **Testes** do núcleo, para garantir que os fatores e as profundidades continuam certos.

## Qual modo usar

- **2D**, com câmera ortográfica: cada camada tem um fator de movimento e todas ficam na mesma grade de pixels. É o mais indicado para pixel art, como em Celeste e Dead Cells, e é o modo do cenário de exemplo.
- **Perspectiva**, com câmera em perspectiva: as camadas ficam em profundidades de verdade e o parallax vem da própria câmera, nos dois eixos. É o caminho do Hollow Knight, que usa arte pintada. Em pixel art, a escala muda com a distância e os pixels deixam de ter o mesmo tamanho entre as camadas.

## Tecnologias

- **Pacote:** C# e a API de editor da Unity 6, com o Universal Render Pipeline.
- **Efeitos:** shaders próprios, para o vento e o brilho aditivo.
- **Arte do exemplo:** Python com PIL, em pixel art.
- **Testes:** Unity Test Framework.
