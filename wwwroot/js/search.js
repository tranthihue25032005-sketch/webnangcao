document.addEventListener("DOMContentLoaded", function () {
    const searchBox = document.getElementById("searchBox");
    const resultsBox = document.getElementById("searchResults");

    if (!searchBox) return;

    let timeout = null;

    searchBox.addEventListener("keyup", function () {
        clearTimeout(timeout);

        const keyword = this.value;

        timeout = setTimeout(() => {
            if (!keyword) {
                resultsBox.innerHTML = "";
                return;
            }

            fetch(`/Client/Search?keyword=${keyword}`)
                .then(res => res.json())
                .then(data => {
                    resultsBox.innerHTML = "";

                    if (data.length === 0) {
                        resultsBox.innerHTML = `<div class="list-group-item">Không tìm thấy</div>`;
                        return;
                    }

                    data.forEach(item => {
                        resultsBox.innerHTML += `
                            <a href="/Client/Product/${item.id}" 
                               class="list-group-item list-group-item-action d-flex align-items-center gap-2">
                                <img src="${item.image ?? '/images/no-image.png'}" 
                                     width="40" height="40" style="object-fit: cover;">
                                <div>
                                    <div>${item.name}</div>
                                    <small class="text-danger">
                                        ${item.price.toLocaleString('vi-VN')} ₫
                                    </small>
                                </div>
                            </a>
                        `;
                    });
                });
        }, 300);
    });
});