using Fusion;
using Fusion.StatsInternal;
using UnityEngine;
using UnityEngine.UI;

public class FusionStatsObjectIds : Fusion.Behaviour, IFusionStatsView
{
	protected const int PAD = 10;

	protected const int MARGIN = 6;

	[SerializeField]
	[HideInInspector]
	private Text _inputValueText;

	[SerializeField]
	[HideInInspector]
	private Text _stateValueText;

	[SerializeField]
	[HideInInspector]
	private Text _objectIdLabel;

	[SerializeField]
	[HideInInspector]
	private Image _stateAuthBackImage;

	[SerializeField]
	[HideInInspector]
	private Image _inputAuthBackImage;

	private FusionStats _fusionStats;

	private static Color _noneAuthColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);

	private static Color _inputAuthColor = new Color(0.1f, 0.6f, 0.1f, 1f);

	private static Color _stateAuthColor = new Color(0.8f, 0.4f, 0f, 1f);

	private const float LABEL_DIVIDING_POINT = 0.7f;

	private const float TEXT_PAD = 4f;

	private const float TEXT_PAD_HORIZ = 6f;

	private const int MAX_TAG_FONT_SIZE = 18;

	private bool _previousHasInputAuth;

	private bool _previousHasStateAuth;

	private int _previousInputAuthValue = -2;

	private int _previousStateAuthValue = -2;

	private uint _previousObjectIdValue;

	private void Awake()
	{
		_fusionStats = GetComponentInParent<FusionStats>();
	}

	void IFusionStatsView.Initialize()
	{
	}

	public static RectTransform Create(RectTransform parent, FusionStats fusionStats)
	{
		RectTransform rectTransform = parent.CreateRectTransform("Object Ids Panel").ExpandTopAnchor(6f);
		FusionStatsObjectIds fusionStatsObjectIds = rectTransform.gameObject.AddComponent<FusionStatsObjectIds>();
		fusionStatsObjectIds._fusionStats = fusionStats;
		fusionStatsObjectIds.Generate();
		return rectTransform;
	}

	public void Generate()
	{
		Color fontColor = _fusionStats.FontColor;
		RectTransform parent = base.transform.CreateRectTransform("IDs Layout").ExpandAnchor().AddCircleSprite(_fusionStats.ObjDataBackColor);
		RectTransform parent2 = parent.CreateRectTransform("Object Id Panel", expand: true).ExpandTopAnchor().SetAnchors(0f, 0.4f, 0f, 1f);
		parent2.CreateRectTransform("Object Id Label").SetAnchors(0f, 1f, 0.7f, 1f).SetOffsets(6f, -6f, 0f, -4f)
			.AddText("OBJECT ID", TextAnchor.MiddleCenter, fontColor)
			.resizeTextMaxSize = 18;
		RectTransform rt = parent2.CreateRectTransform("Object Id Value").SetAnchors(0f, 1f, 0f, 0.7f).SetOffsets(6f, -6f, 4f, 0f);
		_objectIdLabel = rt.AddText("00", TextAnchor.MiddleCenter, fontColor);
		AddAuthorityPanel(parent, "Input", ref _inputValueText, ref _inputAuthBackImage).SetAnchors(0.4f, 0.7f, 0f, 1f);
		AddAuthorityPanel(parent, "State", ref _stateValueText, ref _stateAuthBackImage).SetAnchors(0.7f, 1f, 0f, 1f);
	}

	private RectTransform AddAuthorityPanel(RectTransform parent, string label, ref Text valueText, ref Image backImage)
	{
		Color fontColor = _fusionStats.FontColor;
		RectTransform rectTransform = parent.CreateRectTransform(label + " Id Panel", expand: true).ExpandTopAnchor().SetAnchors(0.5f, 1f, 0f, 1f)
			.AddCircleSprite(_noneAuthColor, out backImage);
		rectTransform.CreateRectTransform(label + " Label").SetAnchors(0f, 1f, 0.7f, 1f).SetOffsets(6f, -6f, 0f, -4f)
			.AddText(label, TextAnchor.MiddleCenter, fontColor)
			.resizeTextMaxSize = 18;
		RectTransform rt = rectTransform.CreateRectTransform(label + " Value").SetAnchors(0f, 1f, 0f, 0.7f).SetOffsets(6f, -6f, 4f, 0f);
		valueText = rt.AddText("P0", TextAnchor.MiddleCenter, fontColor);
		return rectTransform;
	}

	void IFusionStatsView.CalculateLayout()
	{
	}

	void IFusionStatsView.Refresh()
	{
		if (_fusionStats == null)
		{
			return;
		}
		NetworkObject @object = _fusionStats.Object;
		if (@object == null)
		{
			return;
		}
		if (@object.IsValid)
		{
			bool hasInputAuthority = @object.HasInputAuthority;
			if (_previousHasInputAuth != hasInputAuthority)
			{
				_inputAuthBackImage.color = (hasInputAuthority ? _inputAuthColor : _noneAuthColor);
				_previousHasInputAuth = hasInputAuthority;
			}
			bool flag = @object.HasStateAuthority || @object.Runner.IsServer;
			if (_previousHasStateAuth != flag)
			{
				_stateAuthBackImage.color = (flag ? _stateAuthColor : _noneAuthColor);
				_previousHasStateAuth = flag;
			}
			PlayerRef inputAuthority = @object.InputAuthority;
			if (_previousInputAuthValue != inputAuthority)
			{
				_inputValueText.text = ((inputAuthority == -1) ? "-" : ("P" + inputAuthority.PlayerId));
				_previousInputAuthValue = inputAuthority;
			}
			PlayerRef stateAuthority = @object.StateAuthority;
			if (_previousStateAuthValue != stateAuthority)
			{
				_stateValueText.text = ((stateAuthority == -1) ? "-" : ("P" + stateAuthority.PlayerId));
				_previousStateAuthValue = stateAuthority;
			}
		}
		uint raw = @object.Id.Raw;
		if (raw != _previousObjectIdValue)
		{
			_objectIdLabel.text = raw.ToString();
			_previousObjectIdValue = raw;
		}
	}
}
