using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using KSP.Localization;
using TMPro;

using KodeUI;

namespace KerbalKonstructs.UI {

	public class StorageItemView : LayoutPanel
	{
		StorageItem storage;

		public class StorageItemViewEvent : UnityEvent<StorageItem> { }
		StorageItemViewEvent onIncrementEvent;
		StorageItemViewEvent onDecrementEvent;

		UIText resourceName;
		InfoLine stored;
		InfoLine volume;
		InfoLine vessel;
		TransferButtons transferButtons;

		public override void CreateUI()
		{
			base.CreateUI ();

			onIncrementEvent = new StorageItemViewEvent ();
			onDecrementEvent = new StorageItemViewEvent ();

			var storedMin = new Vector2(0, 0);
			var storedMax = new Vector2(0.48f, 1);
			var vesselMin = new Vector2(0.52f, 0);
			var vesselMax = new Vector2(1, 1);

			this.Vertical ()
				.ChildAlignment(TextAnchor.MiddleCenter)
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.Add<UIText>(out resourceName)
					.Finish()
				.Add<InfoLine>(out volume)
					.Label(KKLocalization.StorageVolume)
					.Finish()
				.Add<LayoutAnchor>()
					.DoPreferredWidth (true)
					.DoPreferredHeight (true)
					.FlexibleLayout(true, false)
					.Add<InfoLine>(out stored)
						.Label(KKLocalization.StorageStored)
						.Anchor(storedMin, storedMax)
						.Finish()
					.Add<InfoLine>(out vessel)
						.Label(KKLocalization.StorageHeld)
						.Anchor(vesselMin, vesselMax)
						.Finish()
					.Finish()
				.Add<TransferButtons>(out transferButtons)
					.OnIncrement(onIncrement)
					.OnDecrement(onDecrement)
					.Finish()
				.Finish();
		}

		void onIncrement ()
		{
			Debug.Log($"[StorageItemView] onIncrement {storage.name}");
			onIncrementEvent.Invoke (storage);
		}

		void onDecrement ()
		{
			Debug.Log($"[StorageItemView] onDecrement {storage.name}");
			onDecrementEvent.Invoke (storage);
		}

		public StorageItemView OnIncrement (UnityAction<StorageItem> action)
		{
			onIncrementEvent.AddListener (action);
			return this;
		}

		public StorageItemView OnDecrement (UnityAction<StorageItem> action)
		{
			onDecrementEvent.AddListener (action);
			return this;
		}

		public StorageItemView Storage (StorageItem storage)
		{
			this.storage = storage;
			resourceName.Text(storage.name);
			stored.Info($"{storage.storedAmount:F2}");
			volume.Info($"{storage.storedVolume:F2} / {storage.maxVolume:F2}");
			if (storage.vesselResource != null) {
				transferButtons.SetActive(true);
				vessel.SetActive(true);
				vessel.Info($"{storage.vesselResource.amount:F2} / {storage.vesselResource.maxAmount:F2}");
			} else {
				transferButtons.SetActive(false);
				vessel.SetActive(false);
			}
			return this;
		}
	}
}
