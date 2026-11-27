using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations; // <-- Adicione esta linha

namespace TechToysDominio
{
    public class Peca : Item
    {
        public double Peso_Impressao { get; set; } //Gramas
        public double Tempo_Impressao { get; set; } //Minutos
        public double largura { get; set; }
        public double comprimento { get; set; }
        public double altura { get; set; }
        // Relação antiga mantida para compatibilidade com registros e integrações anteriores.
        public MateriaPrima? Materia_Prima { get; set; }
        public List<PecaMateriaPrima> MateriasPrimas { get; set; } = new();
        public Impressora? Impressora { get; set; }
        public bool Ativo { get; set; }

        public override string ToString()
        {
            var materiais = MateriasPrimas.Count > 0
                ? string.Join(", ", MateriasPrimas.Select(x => $"{x.MateriaPrima?.Nome}: {x.Peso:N3}g"))
                : Materia_Prima?.Nome ?? "Não informada";
            return $"[ID: {Id}] Nome: {Nome} | Peso da impressão: {Peso_Impressao} | Tempo da Impressão: {Tempo_Impressao} | Dimensões(L,C,A): ({largura},{comprimento},{altura}) | Matérias-primas: {materiais} | Impressora: {Impressora?.Modelo ?? "Não informada"} [{Impressora?.Id}] | Ativo: {Ativo}";
        }
        public Peca()
        {
            Unidade_Medida = "UN";
            Ativo = true;
        }

        public double Calcular_Custo_Materia_Prima()
        {
            if (MateriasPrimas.Count > 0)
                return MateriasPrimas.Sum(x => x.Peso * (x.MateriaPrima?.Calcular_Preco_Grama() ?? 0));
            return Materia_Prima == null ? 0 : Peso_Impressao * Materia_Prima.Calcular_Preco_Grama();
        }
        public double Calcular_Custo_Impressao()
        {
            return Impressora == null ? 0 : Impressora.Calcular_Custo_Impressao(Tempo_Impressao);
        }
        public double Calcular_Custo_Total()
        {
            return Calcular_Custo_Impressao() + Calcular_Custo_Materia_Prima();
        }
    }
}
