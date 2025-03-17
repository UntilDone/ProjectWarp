using System;

public class card_data
{
    public int id { get; set; }
    public string card_name { get; set; }
    public string sprite_name { get; set; }
    public string type { get; set; }
    public string part { get; set; }
    public int part_burden { get; set; }
    public string rarity { get; set; }
    public int stat { get; set; }
    public string desc { get; set; }
    public int attack_count { get; set; }
    public int draw_count { get; set; }
    public string status_name { get; set; }
    public int status_duration { get; set; }
    public string status_target { get; set; }
    public bool unlock { get; set; }
    public bool targetable { get; set; }
    public bool playable { get; set; }
    public bool defend { get; set; }
}
