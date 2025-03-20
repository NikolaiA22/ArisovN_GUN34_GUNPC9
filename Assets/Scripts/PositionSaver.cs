using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace DefaultNamespace
{
	public class PositionSaver : MonoBehaviour
	{

        [Serializable]
		public struct Data
		{
			public Vector3 Position;
			public float Time;
        }

        
        [Tooltip("Create File")]
        public TextAsset _json;

        [field: SerializeField]
        [field: HideInInspector]
        public List<Data> Records { get; private set; } = new List<Data>();

        private void Awake()
		{
			//todo comment: Что будет, если в теле этого условия не сделать выход из метода?
			//при запуске игры, объект выключится и будет выводиться ошибка
			if (_json == null)
			{
				gameObject.SetActive(false);
				Debug.LogError("Please, create TextAsset and add in field _json");
				return;
			}
			
			JsonUtility.FromJsonOverwrite(_json.text, this);
			//todo comment: Для чего нужна эта проверка (что она позволяет избежать)?
			//проверяет произошла ли запись перемещения, если нет, то начнется
			if (Records == null)
				Records = new List<Data>(10);
		}

		private void OnDrawGizmos()
		{
			//todo comment: Зачем нужны эти проверки (что они позволляют избежать)?
			//если запись еще не началась, метод возвращает зеленую сферу
			if (Records == null || Records.Count == 0) return;
			var data = Records;
			var prev = data[0].Position;
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(prev, 0.3f);
			//todo comment: Почему итерация начинается не с нулевого элемента?
			//потому что в нулевой момент не происходит никакого перемещения
			for (int i = 1; i < data.Count; i++)
			{
				var curr = data[i].Position;
				Gizmos.DrawWireSphere(curr, 0.3f);
				Gizmos.DrawLine(prev, curr);
				prev = curr;
			}
		}

        public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
        [ContextMenu("Create File")]
		private void CreateFile()
		{
			//todo comment: Что происходит в этой строке?
			//создается файл с текущими данными перемещения
			var stream = File.Create(Path.Combine(Application.dataPath, "Path.txt"));
			//todo comment: Подумайте для чего нужна эта строка? (а потом проверьте догадку, закомментировав)
			//при перемещении, при новых координатах, данные обновляются
			stream.Dispose();
			UnityEditor.AssetDatabase.Refresh();
			//В Unity можно искать объекты по их типу, для этого используется префикс "t:"
			//После нахождения, Юнити возвращает массив гуидов (которые в мета-файлах задаются, например)
			var guids = UnityEditor.AssetDatabase.FindAssets("t:TextAsset");
			foreach (var guid in guids)
			{
				//Этой командой можно получить путь к ассету через его гуид
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				//Этой командой можно загрузить сам ассет
				var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                //todo comment: Для чего нужны эти проверки?
                //если ассет есть и его имя Path передаются "грязные" данные, сохраняются и обновляются
                if (asset != null && asset.name == "Path")
				{
					_json = asset;
					UnityEditor.EditorUtility.SetDirty(this);
					UnityEditor.AssetDatabase.SaveAssets();
					UnityEditor.AssetDatabase.Refresh();
					//todo comment: Почему мы здесь выходим, а не продолжаем итерироваться?
					//
					return;
				}
			}
		}

		[UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
		public class ReadOnlyAttributeDrawer : UnityEditor.PropertyDrawer
		{
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                GUI.enabled = false;
				UnityEditor.EditorGUI.PropertyField(position, property, label);
				GUI.enabled = true;
            }
        }

        private void OnDestroy()
		{
            string jsonData = JsonUtility.ToJson(this, true); 

            string filePath = Path.Combine(Application.persistentDataPath, "Path.json");

            File.WriteAllText(filePath, jsonData);

            Debug.Log("Данные успешно записаны в файл Path.json");
        }
#endif
    }
}