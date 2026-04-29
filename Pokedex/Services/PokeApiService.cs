using PokeApiNet;  
using Pokedex.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pokedex.Services;

public class PokeApiService
{

    // PokeApiNet baut HTTP-Request & empfängt json und wandelt in Pokemon Objekt um
    private PokeApiClient _client = new PokeApiClient();

    public async Task<PokemonModel?> GetPokemonAsync(string nameOrId)
    {
        try
        {
            Pokemon pokemon = await _client.GetResourceAsync<Pokemon>(
                nameOrId.ToLower().Trim()
            );

            // Deutschen Pokémon-Namen holen 2.API Aufruf, Schleife géht durhc alle einträge und speichert de
            PokemonSpecies species = await _client.GetResourceAsync<PokemonSpecies>(pokemon.Id);
            string name = string.Empty;
            foreach (Names entry in species.Names)
            {
                if (entry.Language.Name == "de")
                    name = entry.Name;
            }

            // Types befüllen
            List<string> types = new List<string>();
            foreach (PokemonType pokemonType in pokemon.Types)
            {
                types.Add(pokemonType.Type.Name);
            }

            // Moves befüllen (nur die ersten 4 — deutsch)
            List<string> moves = new List<string>();
            int moveCount = 0;
            foreach (PokemonMove pokemonMove in pokemon.Moves)
            {
                if (moveCount >= 4) break; // Max 4 Moves da Pokemon bis zu 30 Moves haben
                Move move = await _client.GetResourceAsync<Move>(pokemonMove.Move.Name);
                foreach (Names entry in move.Names)  // Filtern nach Deutschen Namen
                {
                    if (entry.Language.Name == "de")
                        moves.Add(entry.Name);
                }
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
                Name = name,
                Types = types,
                Moves = moves,
                Hp = hp,
                Attack = attack,
                Defense = defense,
                SpecialAttack = specialAttack,
                SpecialDefense = specialDefense,
                Speed = speed,
                SpriteUrl = pokemon.Sprites.FrontDefault,
                SoundUrl = $"https://raw.githubusercontent.com/PokeAPI/cries/main/cries/pokemon/latest/{pokemon.Id}.ogg",
            };
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private string Capitalize(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return char.ToUpper(text[0]) + text.Substring(1);
    }
}