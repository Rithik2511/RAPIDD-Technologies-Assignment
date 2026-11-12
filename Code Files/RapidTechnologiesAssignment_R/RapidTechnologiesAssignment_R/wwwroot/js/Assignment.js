let tableData = [];
let take = 50;
let skip = 0;
async function fetchTableData() {
    try {
        const response = await fetch(`Home/FetchTableData?skip=${skip}&take=${take}`);
        if (!response.ok) {
            throw new Error(response.status);
        }
        tableData = await response.json();
    } catch (error) {
        console.error(error);
    }
}


async function CreateGrid() {
    let timer = 0;
    const minimumWorkingHours = 100;
    await fetchTableData();
    if (!tableData && !tableData.length) {
        alert("Data Not Fecthed From Backend");
    }
    const gridDiv = document.getElementById("grid");
    gridDiv.replaceChildren();
    const gridWidth = grid.clientWidth;
    const columnHeaders = Object.keys(tableData[0]);
    const tableHeader = document.createElement("tr");
    const individualColumnWidth = gridWidth / columnHeaders.length;
    for (let columnIndex = 0; columnIndex < columnHeaders.length; columnIndex++) {
        let tableTD = document.createElement("td");
        tableTD.innerHTML = `<span>${columnHeaders[columnIndex]}</span><span><i class="fa-solid fa-filter"></i></span>`
        tableTD.style.width = `${individualColumnWidth}px`;
        tableTD.style.padding = `20px`;
        tableTD.style.display = 'flex';
        tableTD.style.border = '1px solid black';
        tableTD.style.justifyContent = 'space-between';
        tableHeader.appendChild(tableTD);
    }
    tableHeader.style.display = 'flex';
    gridDiv.appendChild(tableHeader);
    for (let rowIndex = 0; rowIndex < tableData.length; rowIndex++) {
        const tableRow = document.createElement("tr");
        let minimumHoursWorked = false;
        for (let columnIndex = 0; columnIndex < columnHeaders.length; columnIndex++) {
            let tableTD = document.createElement("td");
            let dataToAddInCell = tableData[rowIndex][columnHeaders[columnIndex]];
            tableTD.innerHTML = `<span>${dataToAddInCell}</span>`
            tableTD.style.width = `${individualColumnWidth}px`;
            tableTD.style.padding = `10px`;
            tableTD.style.display = 'flex';
            tableTD.style.border = '1px solid black';
            if (columnHeaders[columnIndex] === 'TotalTimeInHours' && !isNaN(dataToAddInCell) && dataToAddInCell >= minimumWorkingHours) {
                minimumHoursWorked = true;
            }
            tableRow.appendChild(tableTD);
        }
        timer += 50;
        setTimeout(() => {
            tableRow.style.display = 'flex';
            if (!minimumHoursWorked) {
                tableRow.style.backgroundColor = "lightcoral";
            }
            gridDiv.appendChild(tableRow);
        }, timer);
    }
    //let footerRow = document.createElement('tr');

}

async function CreateChart() {
    const chartElement = document.getElementById("PieChart");
    if (chartElement) {
        chartElement.replaceChildren();
        const imageElement = document.createElement("img");
        imageElement.alt = "Dynamic Pie Chart";
        imageElement.style.border = "1px solid #ccc";
        imageElement.src = "/Home/CreateChart";
        chartElement.appendChild(imageElement);
    }
    
}
