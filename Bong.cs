using System;
using System.Collections.Generic;
using UnityEngine;

public class Bong : MonoBehaviour, Hoverable, Interactable
{
	private ZNetView m_nview;

	private Piece m_piece;

	[Header("Bong")]
	public string m_name = "$piece_bong";

	public float m_startFuel = 3f;

	public float m_maxFuel = 10f;

	public float m_secPerFuel = 3f;

	public float m_checkTerrainOffset = 0.2f;

	public float m_coverCheckOffset = 0.5f;

	private const float m_minimumOpenSpace = 0.5f;

	public GameObject m_enabledObject;

	public GameObject m_enabledObjectLow;

	public GameObject m_enabledObjectHigh;

	public GameObject m_playerBaseObject;

	public ItemDrop m_fuelItem;

	public SmokeSpawner m_smokeSpawner;

	public EffectList m_fuelAddedEffects = new EffectList();
	public EffectList m_smokedEffects = new EffectList();

	[Header("Fireworks")]
	public ItemDrop m_fireworkItem;

	public int m_fireworkItems = 2;

	public GameObject m_fireworks;

	public float ttl;

	private bool m_blocked;

	private bool m_wet;

	private static int m_solidRayMask;

	public void Awake()
	{
		m_nview = base.gameObject.GetComponent<ZNetView>();
		m_piece = base.gameObject.GetComponent<Piece>();
		if (m_nview.GetZDO() == null)
		{
			return;
		}
		if (m_solidRayMask == 0)
		{
			m_solidRayMask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "terrain");
		}
		if (m_nview.IsOwner() && m_nview.GetZDO().GetFloat("fuel", -1f) == -1f)
		{
			m_nview.GetZDO().Set("fuel", m_startFuel);
			if (m_startFuel > 0f)
			{
				m_fuelAddedEffects.Create(base.transform.position, base.transform.rotation);
			}
		}
		m_nview.Register("AddFuel", RPC_AddFuel);
		//InvokeRepeating("UpdateFireplace", 0f, 2f);
		InvokeRepeating("CheckEnv", 4f, 4f);
	}

	private void Start()
	{
		//if ((bool)m_playerBaseObject && (bool)m_piece)
		//{
		//	m_playerBaseObject.SetActive(m_piece.IsPlacedByPlayer());
		//}
	}

	private double GetTimeSinceLastUpdate()
	{
		DateTime time = ZNet.instance.GetTime();
		DateTime d = new DateTime(m_nview.GetZDO().GetLong("lastTime", time.Ticks));
		TimeSpan timeSpan = time - d;
		m_nview.GetZDO().Set("lastTime", time.Ticks);
		double num = timeSpan.TotalSeconds;
		if (num < 0.0)
		{
			num = 0.0;
		}
		return num;
	}

	private void UpdateFireplace()
	{
		if (!m_nview.IsValid())
		{
			return;
		}
		if (m_nview.IsOwner())
		{
			//float @float = m_nview.GetZDO().GetFloat("fuel");
			//double timeSinceLastUpdate = GetTimeSinceLastUpdate();
			//if (IsBurning())
			//{
			//	float num = (float)(timeSinceLastUpdate / (double)m_secPerFuel);
			//	@float -= num;
			//	if (@float <= 0f)
			//	{
			//		@float = 0f;
			//	}
			//	m_nview.GetZDO().Set("fuel", @float);
			//}
		}
		UpdateState();
	}

	private void CheckEnv()
	{
		CheckUnderTerrain();
		if (m_enabledObjectLow != null && m_enabledObjectHigh != null)
		{
			CheckWet();
		}
	}

	private void CheckUnderTerrain()
	{
		m_blocked = false;
		RaycastHit hitInfo;
		if (Heightmap.GetHeight(base.transform.position, out var height) && height > base.transform.position.y + m_checkTerrainOffset)
		{
			m_blocked = true;
		}
		else if (Physics.Raycast(base.transform.position + Vector3.up * m_coverCheckOffset, Vector3.up, out hitInfo, 0.5f, m_solidRayMask))
		{
			m_blocked = true;
		}
		else if ((bool)m_smokeSpawner && m_smokeSpawner.IsBlocked())
		{
			m_blocked = true;
		}
	}

	private void CheckWet()
	{
		Cover.GetCoverForPoint(base.transform.position + Vector3.up * m_coverCheckOffset, out var coverPercentage, out var underRoof);
		m_wet = false;
		if (EnvMan.instance.GetWindIntensity() >= 0.8f && coverPercentage < 0.7f)
		{
			m_wet = true;
		}
		if (EnvMan.instance.IsWet() && !underRoof)
		{
			m_wet = true;
		}
	}

	private void UpdateState()
	{
		//if (IsBurning())
		//{
		//	//m_enabledObject.SetActive(value: true);
		//	//if ((bool)m_enabledObjectHigh && (bool)m_enabledObjectLow)
		//	//{
		//	//	m_enabledObjectHigh.SetActive(!m_wet);
		//	//	m_enabledObjectLow.SetActive(m_wet);
		//	//}
		//}
		//else
		//{
		//	//m_enabledObject.SetActive(value: false);
		//	//if ((bool)m_enabledObjectHigh && (bool)m_enabledObjectLow)
		//	//{
		//	//	m_enabledObjectLow.SetActive(value: false);
		//	//	m_enabledObjectHigh.SetActive(value: false);
		//	//}
		//}
	}

	public string GetHoverText()
	{
		float @float = m_nview.GetZDO().GetFloat("fuel");
		return Localization.instance.Localize(m_name + " ( $piece_fire_fuel " + Mathf.Ceil(@float) + "/" + (int)m_maxFuel + " )\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use " + m_fuelItem.m_itemData.m_shared.m_name + "\n[<color=yellow><b>1-8</b></color>] $piece_useitem");
	}

	public string GetHoverName()
	{
		return m_name;
	}

	public bool Interact(Humanoid user, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (!m_nview.HasOwner())
		{
			m_nview.ClaimOwnership();
		}
		Inventory inventory = user.GetInventory();
		if (inventory != null)
		{
			Vector3 bongSmokePos = base.transform.position;
			bongSmokePos.y += (float)1.65;

			//Vector3 mouthSmokePos = user.transform.position;
			//mouthSmokePos.y += (float)0.825;

			//Vector3 mouthSmokePos = user.m_head.transform.position;
			//Quaternion mouthSmokeAngle = user.m_head.rotation;
			//mouthSmokeAngle.eulerAngles.Set(0, 0, 90);

			// Player has buds, bong is empty
			if (inventory.HaveItem(m_fuelItem.m_itemData.m_shared.m_name))
			{
				if ((float)Mathf.CeilToInt(m_nview.GetZDO().GetFloat("fuel")) >= m_maxFuel)
				{
					user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$bong_effectstart", m_fuelItem.m_itemData.m_shared.m_name));
					user.m_seman.AddStatusEffect("BongStatusEffect", true);
					// play bong sound / smoke effect
					m_fuelAddedEffects.Create(bongSmokePos, base.transform.rotation);
					//m_smokedEffects.Create(mouthSmokePos, user.transform.rotation);

					float @float = m_nview.GetZDO().GetFloat("fuel");
					@float -= 1;
                    if (@float <= 0f)
                    {
                        @float = 0f;
                    }
                    m_nview.GetZDO().Set("fuel", @float);
					base.transform.Find("_enabled").gameObject.SetActive(false);

					// Rested Management
					Player player = user as Player;
					List<StatusEffect> statlist = player.GetSEMan().m_statusEffects;
					bool isRested = false;
					for (int i = 0; i < statlist.Count; i++)
					{
						if (statlist[i].GetType() == typeof(SE_Rested))
						{
                            // Set the current rested max time to (max time - time elapsed + 5m) and reset timer
                            statlist[i].m_ttl = (statlist[i].m_ttl - statlist[i].m_time) + ttl;
                            statlist[i].m_time = 0;
                            isRested = true;

                            break;
						}
					}
					if (!isRested)
						player.m_seman.AddStatusEffect("Rested", true);
					return true;
				}
				user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$bong_addbud", m_fuelItem.m_itemData.m_shared.m_name));
				inventory.RemoveItem(m_fuelItem.m_itemData.m_shared.m_name, 1);
                base.transform.Find("_enabled").gameObject.SetActive(true);
				m_nview.InvokeRPC("AddFuel");
				return true;
			}
			// Player doesn't have any more buds, but the bong is filled
			else if ((float)Mathf.CeilToInt(m_nview.GetZDO().GetFloat("fuel")) >= m_maxFuel)
            {
				user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$bong_effectstart", m_fuelItem.m_itemData.m_shared.m_name));
				user.m_seman.AddStatusEffect("BongStatusEffect", true);
				// play bong sound / smoke effect
				m_fuelAddedEffects.Create(bongSmokePos, base.transform.rotation);
				//m_smokedEffects.Create(mouthSmokePos, user.transform.rotation);

				float @float = m_nview.GetZDO().GetFloat("fuel");
				@float -= 1;
				if (@float <= 0f)
				{
					@float = 0f;
				}
				m_nview.GetZDO().Set("fuel", @float);
				base.transform.Find("_enabled").gameObject.SetActive(false);

				// Rested Management
				Player player = user as Player;
				List<StatusEffect> statlist = player.GetSEMan().m_statusEffects;
				bool isRested = false;
				for (int i = 0; i < statlist.Count; i++)
				{
					if (statlist[i].GetType() == typeof(SE_Rested))
					{
						// Set the current rested max time to (max time - time elapsed + 10m) and reset timer
						statlist[i].m_ttl = (statlist[i].m_ttl - statlist[i].m_time) + ttl;
						statlist[i].m_time = 0;
						isRested = true;
						break;
					}
				}
				if (!isRested)
					player.m_seman.AddStatusEffect("Rested", true);
				return true;
			}
			user.Message(MessageHud.MessageType.Center, "$msg_outof " + m_fuelItem.m_itemData.m_shared.m_name);
			return false;
		}
		return true;
	}

	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (item.m_shared.m_name == m_fuelItem.m_itemData.m_shared.m_name)
		{
			if ((float)Mathf.CeilToInt(m_nview.GetZDO().GetFloat("fuel")) >= m_maxFuel)
			{
				// Added 0.2.1 to fix the 1/1 weed buds error
				//user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantaddmore", item.m_shared.m_name));
				//user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$bong_effectstart", m_fuelItem.m_itemData.m_shared.m_name));
				//user.m_seman.AddStatusEffect("BongStatusEffect", true);
				//// play bong sound
				//m_fuelAddedEffects.Create(base.transform.position, base.transform.rotation);
				//float @float = m_nview.GetZDO().GetFloat("fuel");
				//@float -= 1;
				//if (@float <= 0f)
				//{
				//	@float = 0f;
				//}
				//m_nview.GetZDO().Set("fuel", @float);
				//return true;
			}
			Inventory inventory = user.GetInventory();
			user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", item.m_shared.m_name));
			inventory.RemoveItem(item, 1);
			m_nview.InvokeRPC("AddFuel");
			return true;
		}
		return false;
	}

	private void RPC_AddFuel(long sender)
	{
		if (m_nview.IsOwner())
		{
			float @float = m_nview.GetZDO().GetFloat("fuel");
			if (!((float)Mathf.CeilToInt(@float) >= m_maxFuel))
			{
				@float = Mathf.Clamp(@float, 0f, m_maxFuel);
				@float += 1f;
				@float = Mathf.Clamp(@float, 0f, m_maxFuel);
				m_nview.GetZDO().Set("fuel", @float);
				//m_fuelAddedEffects.Create(base.transform.position, base.transform.rotation);
				UpdateState();
			}
		}
	}

	public bool CanBeRemoved()
	{
		return true;
	}

	//public bool IsBurning()
	//{
	//	if (m_blocked)
	//	{
	//		return false;
	//	}
	//	float waterLevel = WaterVolume.GetWaterLevel(m_enabledObject.transform.position);
	//	if (m_enabledObject.transform.position.y < waterLevel)
	//	{
	//		return false;
	//	}
	//	return m_nview.GetZDO().GetFloat("fuel") > 0f;
	//}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(base.transform.position + Vector3.up * m_coverCheckOffset, 0.5f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(base.transform.position + Vector3.up * m_checkTerrainOffset, new Vector3(1f, 0.01f, 1f));
	}
}
