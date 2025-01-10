using System.Collections.Generic;
using Plugins.CityCommon.Msg;

public class SimAgentStateSpeak: ISimAgentState
{
    private List<string> words_list;
    private int index_of_words;
    private float speak_duration;
    private List<AttrChange> attr_change;
    private string song;
    private int content_type;

    public override void Reuse()
    {
        base.Reuse();
        words_list = null;
        index_of_words = 0;
        speak_duration = 0;
        content_type = 0;
        attr_change = null;
        song = null;
    }
    public SimAgentStateSpeak Init(List<string> words_list, int index_of_words, float speak_duration, int content_type, List<AttrChange> attr_change, string song)
    {
        this.words_list = words_list;
        this.index_of_words = index_of_words;
        this.speak_duration = speak_duration;
        this.attr_change = attr_change;
        this.song = song;
        this.content_type = content_type;
        return this;
    }
    
    protected override void OnEnter()
    {
        base.OnEnter();
        if (speak_duration > 0)
        {
            _duration = speak_duration;
        }
        else
        {
            _duration = MyRandom.Between(5, 10);
        }

        npc.ShowWordsText(words_list[index_of_words], content_type, false);

        if (index_of_words == 0)
        {
            npc.StartSong(song);
        }
    }

    protected override void OnTimeout()
    {
        base.OnTimeout();
        index_of_words++;
        if (index_of_words < words_list.Count)
        {
            npc.ChangeState(city_map.npc_state_pool.Get<SimAgentStateSpeak>().Init(words_list, index_of_words, speak_duration, content_type, attr_change, song));
        }
        else
        {
            if (attr_change != null && attr_change.Count > 0)
            {
                AttrChange t = attr_change[0];
                attr_change.RemoveAt(0);

                npc.ChangeState(city_map.npc_state_pool.Get<SimAgentStateAttrChange>()
                    .Init(attr_change, t.attr, t.value, t.value_change));
            }
            else
            {
                npc.ChangeStateToIdle();
            }
        }
    }

    protected override void OnExit()
    {
        npc.target_pos = Point.zero;
        npc.target_place_guid = 0;
        npc.HideWordsText();
        base.OnExit();
    }
    
    public override string GetDesc()
    {
        if (index_of_words < words_list.Count)
        {
            return "Speak: " + "\n" + words_list[index_of_words];
        }
        else
        {
            return "Speaking";
        }
    }
}