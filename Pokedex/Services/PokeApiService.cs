using PokeApiNet;
using Pokedex.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pokedex.Services
{
    public class PokeApiService
    {
        // Erstellt Wrapper Instanz einmalig
        private PokeApiClient _client = new PokeApiClient();

        public async Task<PokemonModel?> GetPokemonAsync(string nameOrId)
        {
            try
            {
                // API-Call — holt das Pokemon-Objekt vom Wrapper
                Pokemon pokemon = await _client.GetResourceAsync<Pokemon>(
                    nameOrId.ToLower().Trim()
                );

                // Types befüllen
                List<string> types = new List<string>();
                foreach (PokemonType pokemonType in pokemon.Types)
                {
                    types.Add(Capitalize(pokemonType.Type.Name));
                }

                // Moves befüllen (nur die ersten 4)
                List<string> moves = new List<string>();
                int moveCount = 0;
                foreach (PokemonMove pokemonMove in pokemon.Moves)
                {
                    if (moveCount >= 4) break;
                    moves.Add(Capitalize(pokemonMove.Move.Name));
                    moveCount++;
                }

                // Stats befüllen
                int hp = 0, attack = 0, defense = 0;
                int specialAttack = 0, specialDefense = 0, speed = 0;

                foreach (PokemonStat pokemonStat in pokemon.Stats)
                {
                    if (pokemonStat.Stat.Name == "hp") hp = pokemonStat.BaseStat;
                    if (pokemonStat.Stat.Name == "attack") attack = pokemonStat.BaseStat;
                    if (pokemonStat.Stat.Name == "defense") defense = pokemonStat.BaseStat;
                    if (pokemonStat.Stat.Name == "special-attack") specialAttack = pokemonStat.BaseStat;
                    if (pokemonStat.Stat.Name == "special-defense") specialDefense = pokemonStat.BaseStat;
                    if (pokemonStat.Stat.Name == "speed") speed = pokemonStat.BaseStat;
                }

                return new PokemonModel
                {
                    Id = pokemon.Id,
                    Name =Capitalize(pokemon.Name),

                    Types = types,
                    Moves = moves,
                    Hp = hp,
                    Attack = attack,
                    Defense = defense,
                    SpecialAttack = specialAttack,
                    SpecialDefense = specialDefense,
                    Speed = speed,
                    SpriteUrl = pokemon.Sprites.FrontDefault ?? string.Empty,
                    SoundUrl = $"https://raw.githubusercontent.com/PokeAPI/cries/main/cries/pokemon/latest/{pokemon.Id}.ogg",
                };
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        // Hilfsmethode in PokeApiService.cs
        private string Capitalize(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return char.ToUpper(text[0]) + text.Substring(1);
        }
    }
}