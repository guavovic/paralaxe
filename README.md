# Paralaxe

Pacote para a Unity que monta cenários com parallax em camadas, direto no editor, sem escrever código para cada camada. Serve para jogos 2D, em dois modos: um parallax simulado, com um fator de movimento por camada, e um em perspectiva, em que cada camada tem uma profundidade de verdade.

Veio de uma ferramenta de editor simples e virou um pacote, com um cenário de exemplo em pixel art que mostra o que dá para fazer.

## Como foi feito

- **Dois modos de cálculo**, simulado em 2D e em perspectiva, que trocam de um para o outro no mesmo perfil, com a profundidade e o fator equivalentes calculados um a partir do outro.
- **Valores globais** de velocidade e de vento, que o jogador pode afetar e que também afetam o jogador.
- **Efeitos por camada**: desfoque, brilho aditivo, rolagem automática, influência do vento e repetição horizontal sem emenda.
- **Editor próprio**, em `Window > Parallax`, para criar o parallax a partir de imagens, ordenar as camadas e ver o resultado na hora, sem entrar em Play.
- **Cenário de exemplo** em pixel art, com herói animado, câmera que acompanha na horizontal e na vertical, pássaros, vagalumes, luz e neblina. A arte é gerada por script, o que permite refazer o cenário com outra paleta ou outro bioma.
- **Testes** do núcleo, para garantir que os fatores e as profundidades continuam certos.

## Tecnologias

- **Pacote:** C# e a API de editor da Unity 6, com o Universal Render Pipeline.
- **Efeitos:** shaders próprios, para o vento e o brilho aditivo.
- **Arte do exemplo:** Python com PIL, em pixel art.
- **Testes:** Unity Test Framework.
