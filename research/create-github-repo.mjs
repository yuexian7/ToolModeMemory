// Create public GitHub repo under yuexian7.
// Token from Windows git credential helper (git:https://github.com).
import { execFileSync } from "child_process";
import https from "https";

function readWindowsGitToken() {
	const out = execFileSync("git", ["credential", "fill"], {
		input: "protocol=https\nhost=github.com\n\n",
		encoding: "utf8",
	});
	const m = out.match(/^password=(.*)$/m);
	if (m) return m[1].trim();
	throw new Error("no github token in credential fill");
}

function gh(path, method, body, token) {
	return new Promise((resolve, reject) => {
		const data = body ? JSON.stringify(body) : null;
		const req = https.request(
			{
				hostname: "api.github.com",
				path,
				method,
				headers: {
					Authorization: "Bearer " + token,
					Accept: "application/vnd.github+json",
					"User-Agent": "ToolModeMemory-publisher",
					"X-GitHub-Api-Version": "2022-11-28",
					...(data
						? { "Content-Type": "application/json", "Content-Length": Buffer.byteLength(data) }
						: {}),
				},
			},
			(res) => {
				let s = "";
				res.on("data", (c) => (s += c));
				res.on("end", () => {
					let j = null;
					try {
						j = JSON.parse(s);
					} catch {}
					resolve({ status: res.statusCode, body: j || s });
				});
			}
		);
		req.on("error", reject);
		if (data) req.write(data);
		req.end();
	});
}

const token = readWindowsGitToken();
const user = await gh("/user", "GET", null, token);
console.log("github user", user.status, user.body && user.body.login);
if (user.body && user.body.login !== "yuexian7") {
	console.warn("WARNING: token user is", user.body.login, "not yuexian7");
}

const create = await gh(
	"/user/repos",
	"POST",
	{
		name: "ToolModeMemory",
		description:
			"Cities: Skylines II mod — remember tool panel settings per savegame (Tool Mode Memory)",
		private: false,
		auto_init: false,
	},
	token
);
console.log("create repo", create.status, create.body && (create.body.html_url || create.body.message));

if (create.status !== 201 && create.status !== 200) {
	const get = await gh("/repos/yuexian7/ToolModeMemory", "GET", null, token);
	console.log("existing repo", get.status, get.body && get.body.html_url);
}
