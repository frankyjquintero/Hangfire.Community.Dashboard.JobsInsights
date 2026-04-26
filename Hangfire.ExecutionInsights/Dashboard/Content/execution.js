const REFRESH = 5000;

async function load() {
    const queues = document.getElementById('queues').value || "default";
    const jobType = document.getElementById('jobType').value;
    const minutes = document.getElementById('range').value;

    const res = await fetch(`/api/execution/grid?queues=${queues}&jobType=${jobType}&minutes=${minutes}`);
    const data = await res.json();

    render(data);
}

function render(data) {
    const grid = document.getElementById('grid');
    grid.innerHTML = "";

    Object.keys(data).forEach(queue => {
        const row = document.createElement('div');
        row.className = "row-line";

        const label = document.createElement('div');
        label.className = "label";
        label.innerText = queue;

        row.appendChild(label);

        data[queue].forEach(c => {
            const cell = document.createElement('div');
            cell.className = 'cell';

            if (c.failed > 0) cell.classList.add('fail');
            else if (c.processing > 0) cell.classList.add('processing');
            else if (c.succeeded > 0) cell.classList.add('success');
            else cell.classList.add('idle');

            row.appendChild(cell);
        });

        grid.appendChild(row);
    });
}

setInterval(load, REFRESH);

document.querySelectorAll("input, select")
    .forEach(x => x.addEventListener("change", load));

load();
