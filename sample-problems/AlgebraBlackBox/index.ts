import AlgebraEnvironmentSample from "./Environment";

const env = new AlgebraEnvironmentSample();

console.log("starting...");
env.start();

if(typeof document!="undefined")
{
	const output = document.getElementById("output");
	setInterval(()=>{
		if(typeof env.state == "string")
			output!.innerText = env.state;
	},200);
}