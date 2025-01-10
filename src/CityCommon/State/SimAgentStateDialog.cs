using System;
using System.Collections.Generic;
using Plugins.CityCommon.Msg;
using Plugins.CityCommon.Server;

public class SimAgentStateDialog : ISimAgentState
{
    private DecisionDialog dialog;
    private int cur_index_in_dialog;
    private bool is_first_tick = true;
    private string agent_object_name;
    public override string GetDesc()
    {
        return "Chatting";
    }
    public override void Reuse()
    {
        base.Reuse();
        dialog = null;
        cur_index_in_dialog = 0;
        is_first_tick = true;
        agent_object_name = null;
    }
    public SimAgentStateDialog Init(DecisionDialog dialog, int cur_index_in_dialog)
    {
        this.dialog = dialog;
        this.cur_index_in_dialog = cur_index_in_dialog;
        return this;
    }
    protected override void OnEnter()
    {
        base.OnEnter();
        npc.Position = npc.CurPoint.ToMyVector2() + new MyVector2(0.4f, 0);
        SimAgent agent_object = city_map.GetSimAgent(dialog.agent_guid_object);
        if (agent_object != null)
        {
            agent_object_name = agent_object.il_agent.agent_name;
        }
    }

    protected override void OnTick()
    {
        base.OnTick();
        if (is_first_tick)
        {
            is_first_tick = false;
            OnFirstTick();
        }
    }

    private void OnFirstTick()
    {
        if (cur_index_in_dialog < dialog.words_list.Count)
        {
            Words words = dialog.words_list[cur_index_in_dialog];
            string words_content = words.content;
            int display_duration = 3;
            List<string> words_list = new List<string>();
            float speak_duration = 0;
            if (!string.IsNullOrEmpty(words_content))
            {
                string[] arr = words_content.Split("\n");
                foreach (var line in arr)
                {
                    string[] arr2 = line.Split(".");
                    if (arr2.Length > 0)
                    {
                        words_list.AddRange(arr2);
                    }
                }

                speak_duration = GameServerConfig.SPEAK_DURATION_SECONDS_MAX / words_list.Count;
                if (speak_duration > GameServerConfig.SPEAK_INTERVAL_MAX)
                {
                    speak_duration = GameServerConfig.SPEAK_INTERVAL_MAX;
                }

                if (speak_duration < display_duration)
                {
                    speak_duration = display_duration;
                }

                npc.ChangeState(city_map.npc_state_pool.Get<SimAgentStateDialogSpeak>().Init(dialog,
                    cur_index_in_dialog, words_list, 0, speak_duration, words.content_type, words.song));
            }
            else
            {

                cur_index_in_dialog++;
                if (cur_index_in_dialog < dialog.words_list.Count)
                {
                    npc.ChangeState(city_map.npc_state_pool.Get<SimAgentStateDialog>()
                        .Init(dialog, cur_index_in_dialog + 1));
                }
                else
                {
                    npc.ChangeStateToIdle();
                }
            }
        }
        else
        {
            npc.ChangeStateToIdle();
        }

    }
}


public class SimAgentStateDialogSpeak : ISimAgentState
{
    private DecisionDialog dialog;
    private List<string> words_list;
    private int index_of_words;
    private float speak_duration;
    private string song;
    private int content_type;
    private int cur_index_in_dialog;
    private string agent_object_name;
    private string active_agent_name;
    public override string GetDesc()
    {
        return "Chatting";
    }
    public override void Reuse()
    {
        base.Reuse();
        dialog = null;
        words_list = null;
        index_of_words = 0;
        speak_duration = 0;
        content_type = 0;
        song = null;
        cur_index_in_dialog = 0;
        agent_object_name = null;
        active_agent_name = null;
    }
    public SimAgentStateDialogSpeak Init(DecisionDialog dialog, int cur_index_in_dialog, List<string> words_list, int index_of_words, float speak_duration, int content_type, string song)
    {
        this.dialog = dialog;
        this.words_list = words_list;
        this.index_of_words = index_of_words;
        this.speak_duration = speak_duration;
        this.song = song;
        this.content_type = content_type;
        this.cur_index_in_dialog = cur_index_in_dialog;
        return this;
    }
    protected override void OnEnter()
    {
        base.OnEnter();
        SimAgent agent_object = city_map.GetSimAgent(dialog.agent_guid_object);
        if (agent_object != null)
        {
            agent_object_name = agent_object.il_agent.agent_name;
        }

        Words words = dialog.words_list[cur_index_in_dialog];
        active_agent_name = npc.il_agent.agent_name;
        if (words.agent_guid != npc.Guid)
        {
            active_agent_name = agent_object_name;
        }

        base.OnEnter();
        if (speak_duration > 0)
        {
            _duration = speak_duration;
        }
        else
        {
            _duration = MyRandom.Between(5, 10);
        }

        npc.ShowWordsText(words_list[index_of_words], content_type, words.agent_guid != npc.Guid);

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
            npc.ChangeState(city_map.npc_state_pool.Get<SimAgentStateDialogSpeak>().Init(dialog, cur_index_in_dialog, words_list, index_of_words, speak_duration, content_type, song));
        }
        else
        {
            if (cur_index_in_dialog < dialog.words_list.Count)
            {
                npc.ChangeState(city_map.npc_state_pool.Get<SimAgentStateDialog>().Init(dialog, cur_index_in_dialog + 1));
            }
            else
            {
                npc.ChangeStateToIdle();
            }
        }
    }

    protected override void OnExit()
    {
        npc.HideWordsText();
        base.OnExit();
    }
}
