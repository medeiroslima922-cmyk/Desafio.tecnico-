using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

// Execute na pasta do projeto, onde está a pasta dados.
var opcoesJson = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};
var cultura = CultureInfo.GetCultureInfo("pt-BR");

while (true)
{
    Console.WriteLine("\nDESAFIO TARGET SISTEMAS");
    Console.WriteLine("1 — Comissões");
    Console.WriteLine("2 — Estoque");
    Console.WriteLine("3 — Juros");
    Console.WriteLine("0 — Sair");
    Console.Write("Escolha uma opção: ");
    var opcao = Console.ReadLine();
    if (opcao is "0" or null) break;

    try
    {
        switch (opcao)
        {
            case "1": CalcularComissoes(); break;
            case "2": MovimentarEstoque(); break;
            case "3": CalcularJuros(); break;
            default: Console.WriteLine("Opção inválida."); break;
        }
    }
    catch (Exception erro) when (erro is ArgumentException or FormatException
        or OverflowException or IOException or UnauthorizedAccessException or JsonException)
    {
        Console.WriteLine($"Não foi possível concluir: {erro.Message}");
    }
}

// Exercício 1: calcula cada comissão antes de somar por vendedor.
void CalcularComissoes()
{
    var dados = LerJson<DadosVendas>("dados/vendas.json");
    if (dados.Vendas is null) throw new ArgumentException("O JSON deve conter a lista vendas.");
    var totais = new Dictionary<string, decimal>();
    foreach (var venda in dados.Vendas)
    {
        if (venda is null || string.IsNullOrWhiteSpace(venda.Vendedor))
            throw new ArgumentException("Uma venda está sem o nome do vendedor.");
        ValidarValor(venda.Valor);

        decimal taxa;
        if (venda.Valor < 100) taxa = 0;
        else if (venda.Valor < 500) taxa = 0.01m;
        else taxa = 0.05m;

        var comissao = Arredondar(venda.Valor * taxa);
        var nome = venda.Vendedor.Trim();
        if (!totais.ContainsKey(nome)) totais[nome] = 0;
        totais[nome] += comissao;
    }

    Console.WriteLine("\nComissão por vendedor:");
    foreach (var total in totais)
        Console.WriteLine($"{total.Key}: {total.Value.ToString("C2", cultura)}");
}

// Exercício 2: consulta o saldo e registra uma entrada ou saída.
void MovimentarEstoque()
{
    const string arquivoEstado = "dados/estado-estoque.json";
    EstadoEstoque estado;
    if (File.Exists(arquivoEstado))
        estado = LerJson<EstadoEstoque>(arquivoEstado);
    else
        estado = new EstadoEstoque { Produtos = LerJson<DadosEstoque>("dados/estoque.json").Estoque };
    if (estado.Produtos is null || estado.Movimentacoes is null)
        throw new ArgumentException("O JSON de estoque está inválido.");

    Console.WriteLine("\nProdutos disponíveis:");
    foreach (var item in estado.Produtos)
        Console.WriteLine($"{item.CodigoProduto} — {item.DescricaoProduto}: {item.Estoque} unidades");

    var codigo = int.Parse(Perguntar("Código do produto (0 para voltar): "));
    if (codigo == 0) return;
    var produto = estado.Produtos.Find(item => item.CodigoProduto == codigo);
    if (produto is null) throw new ArgumentException("Produto não encontrado.");

    var tipo = Perguntar("Tipo (entrada ou saida): ").ToLowerInvariant();
    if (tipo == "saída") tipo = "saida";
    if (tipo != "entrada" && tipo != "saida")
        throw new ArgumentException("Informe entrada ou saida.");
    var quantidade = int.Parse(Perguntar("Quantidade: "));
    if (quantidade <= 0) throw new ArgumentException("A quantidade deve ser um inteiro positivo.");
    var descricao = Perguntar("Descrição da movimentação: ");

    var saldo = checked(produto.Estoque + (tipo == "entrada" ? quantidade : -quantidade));
    if (saldo < 0) throw new ArgumentException("Estoque insuficiente para esta saída.");
    long ultimoId = 0;
    foreach (var movimento in estado.Movimentacoes)
        ultimoId = Math.Max(ultimoId, movimento.Id);

    var novoMovimento = new Movimento(checked(ultimoId + 1), codigo, tipo, descricao, quantidade, saldo);
    produto.Estoque = saldo;
    estado.Movimentacoes.Add(novoMovimento);

    // Grava saldo e histórico juntos, preservando o JSON inicial.
    File.WriteAllText(arquivoEstado + ".tmp", JsonSerializer.Serialize(estado, opcoesJson));
    File.Move(arquivoEstado + ".tmp", arquivoEstado, overwrite: true);
    Console.WriteLine($"Movimentação nº {novoMovimento.Id}: {descricao}");
    Console.WriteLine($"Estoque final de {produto.DescricaoProduto}: {saldo} unidades.");
}

// Exercício 3: juros simples de 2,5% por dia corrido de atraso.
void CalcularJuros()
{
    var texto = Perguntar("Valor original (ex.: 100,50, sem separador de milhar): ");
    var valor = decimal.Parse(texto.Replace(',', '.'),
        NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
    ValidarValor(valor);
    var vencimento = DateOnly.ParseExact(Perguntar("Vencimento (dd/MM/aaaa): "), "dd/MM/yyyy", cultura);
    var hoje = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(
        DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo")));
    var dias = Math.Max(0, hoje.DayNumber - vencimento.DayNumber);
    var juros = Arredondar(valor * 0.025m * dias);
    Console.WriteLine($"Data do cálculo: {hoje:dd/MM/yyyy}");
    Console.WriteLine($"Dias de atraso: {dias}");
    Console.WriteLine($"Juros: {juros.ToString("C2", cultura)}");
    Console.WriteLine($"Valor total: {(valor + juros).ToString("C2", cultura)}");
}

// Pequenas funções compartilhadas pelos exercícios.
T LerJson<T>(string caminho) =>
    JsonSerializer.Deserialize<T>(File.ReadAllText(caminho), opcoesJson)
    ?? throw new ArgumentException("O arquivo JSON está vazio.");

string Perguntar(string mensagem)
{
    Console.Write(mensagem);
    var resposta = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(resposta))
        throw new ArgumentException("Preencha o campo solicitado.");
    return resposta.Trim();
}

void ValidarValor(decimal valor)
{
    if (valor < 0 || decimal.Round(valor, 2) != valor)
        throw new ArgumentException("Informe um valor não negativo, com até duas casas decimais.");
}

decimal Arredondar(decimal valor) => decimal.Round(valor, 2, MidpointRounding.AwayFromZero);

// Estruturas que representam as informações dos arquivos JSON.
record Venda([property: JsonRequired] string Vendedor, [property: JsonRequired] decimal Valor);
record DadosVendas(List<Venda> Vendas);
record DadosEstoque(List<Produto> Estoque);
record Movimento(long Id, int CodigoProduto, string Tipo, string Descricao, int Quantidade, int EstoqueFinal);

class Produto
{
    public int CodigoProduto { get; set; }
    public string DescricaoProduto { get; set; } = "";
    public int Estoque { get; set; }
}

class EstadoEstoque
{
    public List<Produto> Produtos { get; set; } = [];
    public List<Movimento> Movimentacoes { get; set; } = [];
}
