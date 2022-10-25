using System;

public class Atributo
{
	public Atributo()
	{

        public string type { get; set; }
        public float value { get; set; }

        public Atributo(float value)
        {
            this.type = "Number";
            this.value = value;
        }
    }
}
