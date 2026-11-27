using System;

namespace TechToysDominio
{
    public class VendaEmbalagem
    {
        public int Id { get; set; }
        public int EmbalagemId { get; set; }
        public Embalagem Embalagem { get; set; } = null!;
        public int Quantidade { get; set; }

        public double Calcular_Custo()
        {
            return Embalagem == null ? 0 : Embalagem.Preco * Quantidade;
        }
    }
}
