using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokedex.Models;

public class PokemonModel
{

    public int Id { get; set; }
    public string Name { get; set; } 
    
    public List<string> Types { get; set; }
    public List<string> Moves { get; set; } 
    public int Hp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int SpecialAttack { get; set; }
    public int SpecialDefense { get; set; }
    public int Speed { get; set; }

    public string SpriteUrl { get; set; } 

    public string SoundUrl { get; set; } 


}
