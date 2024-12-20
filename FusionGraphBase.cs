using System;
using Fusion;
using Fusion.StatsInternal;
using UnityEngine;
using UnityEngine.UI;

[ScriptHelp(BackColor = EditorHeaderBackColor.Olive)]
public abstract class FusionGraphBase : Fusion.Behaviour, IFusionStatsView
{
	public enum StatsPer
	{
		Default = 0,
		Individual = 1,
		Tick = 2,
		Second = 4
	}

	protected const int PAD = 10;

	protected const int MRGN = 6;

	protected const int MAX_FONT_SIZE_WITH_GRAPH = 24;

	[SerializeField]
	[HideInInspector]
	protected Text LabelTitle;

	[SerializeField]
	[HideInInspector]
	protected Image BackImage;

	[InlineHelp]
	[SerializeField]
	protected Simulation.Statistics.StatSourceTypes _statSourceType;

	[InlineHelp]
	[SerializeField]
	[CastEnum("CastToStatType")]
	protected int _statId;

	[InlineHelp]
	public StatsPer StatsPerDefault;

	[InlineHelp]
	public float WarnThreshold;

	[InlineHelp]
	public float ErrorThreshold;

	protected IStatsBuffer _statsBuffer;

	protected bool _isOverlay;

	protected FusionStats _fusionStats;

	protected bool _layoutDirty = true;

	protected Simulation.Statistics.StatsPer CurrentPer;

	public Simulation.Statistics.StatSourceInfo StatSourceInfo;

	[SerializeField]
	[HideInInspector]
	private Simulation.Statistics.StatSourceTypes _prevStatSourceType;

	[SerializeField]
	[HideInInspector]
	private int _prevStatId;

	public Simulation.Statistics.StatSourceTypes StateSourceType
	{
		get
		{
			return _statSourceType;
		}
		set
		{
			_statSourceType = value;
			TryConnect();
		}
	}

	public int StatId
	{
		get
		{
			return _statId;
		}
		set
		{
			_statId = value;
			TryConnect();
		}
	}

	public IStatsBuffer StatsBuffer
	{
		get
		{
			if (_statsBuffer == null)
			{
				TryConnect();
			}
			return _statsBuffer;
		}
	}

	public bool IsOverlay
	{
		get
		{
			return _isOverlay;
		}
		set
		{
			if (_isOverlay != value)
			{
				_isOverlay = value;
				CalculateLayout();
				_layoutDirty = true;
			}
		}
	}

	protected virtual Color BackColor
	{
		get
		{
			if (_statSourceType == Simulation.Statistics.StatSourceTypes.Simulation)
			{
				return _fusionStats.SimDataBackColor;
			}
			if (_statSourceType == Simulation.Statistics.StatSourceTypes.NetConnection)
			{
				return _fusionStats.NetDataBackColor;
			}
			return _fusionStats.ObjDataBackColor;
		}
	}

	protected Type CastToStatType
	{
		get
		{
			if (_statSourceType != 0)
			{
				if (_statSourceType != Simulation.Statistics.StatSourceTypes.NetConnection)
				{
					return typeof(Simulation.Statistics.ObjStats);
				}
				return typeof(Simulation.Statistics.NetStats);
			}
			return typeof(Simulation.Statistics.SimStats);
		}
	}

	protected FusionStats LocateParentFusionStats()
	{
		if (_fusionStats == null)
		{
			_fusionStats = GetComponentInParent<FusionStats>();
		}
		return _fusionStats;
	}

	public virtual void Initialize()
	{
	}

	public virtual void CyclePer()
	{
		Simulation.Statistics.StatsPer perFlags = StatSourceInfo.PerFlags;
		switch (CurrentPer)
		{
		case Simulation.Statistics.StatsPer.Individual:
			if ((perFlags & Simulation.Statistics.StatsPer.Tick) == Simulation.Statistics.StatsPer.Tick)
			{
				CurrentPer = Simulation.Statistics.StatsPer.Tick;
			}
			else if ((perFlags & Simulation.Statistics.StatsPer.Second) == Simulation.Statistics.StatsPer.Second)
			{
				CurrentPer = Simulation.Statistics.StatsPer.Second;
			}
			break;
		case Simulation.Statistics.StatsPer.Tick:
			if ((perFlags & Simulation.Statistics.StatsPer.Second) == Simulation.Statistics.StatsPer.Second)
			{
				CurrentPer = Simulation.Statistics.StatsPer.Second;
			}
			else if ((perFlags & Simulation.Statistics.StatsPer.Individual) == Simulation.Statistics.StatsPer.Individual)
			{
				CurrentPer = Simulation.Statistics.StatsPer.Individual;
			}
			break;
		case Simulation.Statistics.StatsPer.Second:
			if ((perFlags & Simulation.Statistics.StatsPer.Individual) == Simulation.Statistics.StatsPer.Individual)
			{
				CurrentPer = Simulation.Statistics.StatsPer.Individual;
			}
			else if ((perFlags & Simulation.Statistics.StatsPer.Tick) == Simulation.Statistics.StatsPer.Tick)
			{
				CurrentPer = Simulation.Statistics.StatsPer.Tick;
			}
			break;
		case Simulation.Statistics.StatsPer.Individual | Simulation.Statistics.StatsPer.Tick:
			break;
		}
	}

	public abstract void CalculateLayout();

	public abstract void Refresh();

	protected virtual bool TryConnect()
	{
		StatSourceInfo = Simulation.Statistics.GetDescription(_statSourceType, _statId);
		if (WarnThreshold == 0f && ErrorThreshold == 0f)
		{
			WarnThreshold = StatSourceInfo.WarnThreshold;
			ErrorThreshold = StatSourceInfo.ErrorThreshold;
		}
		if (!base.gameObject.activeInHierarchy)
		{
			return false;
		}
		if (_fusionStats == null)
		{
			_fusionStats = GetComponentInParent<FusionStats>();
		}
		NetworkRunner networkRunner = _fusionStats?.Runner;
		Simulation.Statistics statistics = networkRunner?.Simulation?.Stats;
		switch (_statSourceType)
		{
		case Simulation.Statistics.StatSourceTypes.Simulation:
			_statsBuffer = statistics?.GetStatBuffer((Simulation.Statistics.SimStats)_statId);
			break;
		case Simulation.Statistics.StatSourceTypes.NetworkObject:
			if (_statId >= 2)
			{
				StatId = 0;
			}
			if (_fusionStats.Object == null)
			{
				_statsBuffer = null;
			}
			else
			{
				_statsBuffer = statistics?.GetObjectBuffer(_fusionStats.Object.Id, (Simulation.Statistics.ObjStats)_statId, createIfMissing: true);
			}
			break;
		case Simulation.Statistics.StatSourceTypes.NetConnection:
			if (networkRunner == null)
			{
				_statsBuffer = null;
			}
			else
			{
				_statsBuffer = statistics?.GetStatBuffer((Simulation.Statistics.NetStats)_statId, networkRunner);
			}
			break;
		default:
			_statsBuffer = null;
			break;
		}
		if ((bool)BackImage)
		{
			BackImage.color = BackColor;
		}
		if ((bool)LabelTitle)
		{
			CheckIfValidIncurrentMode(networkRunner);
			ApplyTitleText();
		}
		if (((uint)StatSourceInfo.PerFlags & (uint)StatsPerDefault) != 0)
		{
			CurrentPer = (Simulation.Statistics.StatsPer)StatsPerDefault;
		}
		else
		{
			CurrentPer = StatSourceInfo.PerDefault;
		}
		return _statsBuffer != null;
	}

	protected void ApplyTitleText()
	{
		Simulation.Statistics.StatSourceInfo statSourceInfo = StatSourceInfo;
		if (statSourceInfo.LongName == null)
		{
			return;
		}
		if (statSourceInfo.InvalidReason != null)
		{
			LabelTitle.text = statSourceInfo.InvalidReason;
			BackImage.gameObject.SetActive(value: false);
			LabelTitle.color = _fusionStats.FontColor * new Color(1f, 1f, 1f, 0.2f);
			return;
		}
		if (LabelTitle.rectTransform.rect.width < 100f)
		{
			LabelTitle.text = statSourceInfo.ShortName ?? statSourceInfo.LongName;
		}
		else
		{
			LabelTitle.text = statSourceInfo.LongName;
		}
		BackImage.gameObject.SetActive(value: true);
	}

	protected void CheckIfValidIncurrentMode(NetworkRunner runner)
	{
		if (!runner)
		{
			return;
		}
		Simulation.Statistics.StatFlags flags = StatSourceInfo.Flags;
		if ((flags & Simulation.Statistics.StatFlags.ValidForBuildType) == 0)
		{
			StatSourceInfo.InvalidReason = "DEBUG DLL ONLY";
			return;
		}
		NetworkObject networkObject = ((_statSourceType != Simulation.Statistics.StatSourceTypes.NetworkObject) ? null : _fusionStats?.Object);
		if ((bool)networkObject && (flags & Simulation.Statistics.StatFlags.ValidOnStateAuthority) == 0 && networkObject.HasStateAuthority)
		{
			StatSourceInfo.InvalidReason = "NON STATE AUTH ONLY";
		}
		else if ((bool)runner)
		{
			if ((flags & Simulation.Statistics.StatFlags.ValidOnServer) == 0 && !runner.IsClient)
			{
				StatSourceInfo.InvalidReason = "CLIENT ONLY";
			}
			else if ((flags & Simulation.Statistics.StatFlags.ValidWithDeltaSnapshot) == 0 && runner.Config.Simulation.ReplicationMode == SimulationConfig.StateReplicationModes.DeltaSnapshots)
			{
				StatSourceInfo.InvalidReason = "EC MODE ONLY";
			}
		}
	}
}
