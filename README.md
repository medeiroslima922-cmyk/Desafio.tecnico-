# Desafio Target Sistemas — C#

Três exercícios em um programa de terminal, com menu e sem bibliotecas externas.

## Como executar

Com o **SDK .NET 8** instalado, abra o terminal na pasta deste projeto e execute:

```bash
dotnet run
```

Escolha uma opção e siga as perguntas na tela:

```text
1 — Comissões
2 — Estoque
3 — Juros
0 — Sair
```

Neste ambiente, o SDK também está instalado na pasta do usuário e o comando
`dotnet` está disponível no terminal, sem precisar executar `export`.

## Arquivos

- `Program.cs`: todo o código, separado por comentários para cada exercício.
- `DesafioTarget.csproj`: configuração do projeto C#.
- `dados/vendas.json`: vendas fornecidas pela empresa.
- `dados/estoque.json`: estoque inicial fornecido pela empresa.
- `.gitignore`: evita enviar arquivos gerados para o Git.

As pastas `bin` e `obj` são criadas automaticamente pelo .NET. Não é necessário editá-las.

## Regras adotadas

**Comissões:** cada venda abaixo de R$ 100 gera zero; de R$ 100 até abaixo de
R$ 500 gera 1%; a partir de R$ 500 gera 5%. Cada comissão é arredondada para
centavos antes da soma por vendedor, com meio centavo para cima. Os resultados
são João: R$ 495,69; Maria: R$ 465,96; Carlos: R$ 379,38; Ana: R$ 404,99.

**Estoque:** escolha o código, informe `entrada` ou `saida`, uma quantidade inteira
positiva e a descrição. O programa mostra o identificador e o saldo final.
Não permite saída maior que o saldo. Para apenas consultar, entre na opção 2 e
digite 0 após a lista. O saldo atualizado e o histórico são salvos automaticamente
em `dados/estado-estoque.json`, preservando os dados iniciais. Os identificadores
são sequenciais dentro desse histórico e continuam após reiniciar o programa.
Use uma instância do programa por vez.

**Juros:** informe o valor com vírgula ou ponto, sem separadores de milhar, e o
vencimento em `dd/MM/aaaa`. A interpretação adotada é juros simples:
`valor × 0,025 × dias de atraso`. Usa dias corridos e a data atual de São Paulo.
Vencimento hoje ou no futuro não gera juros. O resultado é arredondado para
centavos. O enunciado não especifica capitalização nem a regra de arredondamento.
