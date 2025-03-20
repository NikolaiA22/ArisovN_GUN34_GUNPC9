using UnityEngine;

namespace DefaultNamespace
{
	
	[RequireComponent(typeof(PositionSaver))]
	public class EditorMover : MonoBehaviour
	{
		private PositionSaver _save;
		private float _currentDelay;

		//todo comment: Что произойдёт, если _delay > _duration?
		//перемещение не запишется, банально истечет время записи
		[Range(0.2f, 1.0f)]
		private float _delay = 0.5f;

		[Min(0.2f)]
		private float _duration = 5f;

		private void Awake()
		{
			//todo comment: Почему этот поиск производится здесь, а не в начале метода Update?
			//потому что нам сперва надо проверить модем ли мы записывать, апдейт вызывается позже
			_save = GetComponent<PositionSaver>();
			_save.Records.Clear();

            if (_duration < _delay)
            {
                _duration = _delay * 5f;
                //return;
            }
        }

		private void Update()
		{
			_duration -= Time.deltaTime;
			if (_duration <= 0f)
			{
				enabled = false;
				Debug.Log($"<b>{name}</b> finished", this);
				return;
			}
			
			//todo comment: Почему не написать (_delay -= Time.deltaTime;) по аналогии с полем _duration?
			//потому что нам нужно каждый раз отслеживать сколько прошло времени
			_currentDelay -= Time.deltaTime;
			if (_currentDelay <= 0f)
			{
				_currentDelay = _delay;
				_save.Records.Add(new PositionSaver.Data
				{
					Position = transform.position,
					//todo comment: Для чего сохраняется значение игрового времени?
					//потом оно используется для вычеслений в ReplayMovers
					Time = Time.time,
				});
			}
		}
	}
}