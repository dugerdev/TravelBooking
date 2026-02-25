
let showSignup = () => {
    let authBox = document.querySelector('.auth');
    authBox.classList.remove("hide")
    authBox.style.display = 'flex';
    document.querySelector('.signup-page').style.display = 'block';
    document.querySelector('.login-page').style.display = 'none';
}

let showLogin = () => {
    let authBox = document.querySelector('.auth');
    authBox.classList.remove("hide")
    authBox.style.display = 'flex';
    document.querySelector('.login-page').style.display = 'block';
    document.querySelector('.signup-page').style.display = 'none';
}

let closeAuth = () => {
    let authBox = document.querySelector('.auth');
    authBox.classList.add('hide');

    setTimeout(() => {
        document.querySelector('.auth').style.display = 'none';
    }, 500);

}

let signUpform = () => {
    let name = document.querySelector("#name").value.trim();
    let email = document.querySelector("#email").value.trim();
    let num = document.querySelector("#number").value.trim();
    let password = document.querySelector("#password").value.trim();
    let confirmPassword = document.querySelector("#confirm-password").value.trim();

    let errorName = document.querySelector("#error-name");
    let errorEmail = document.querySelector("#error-email");
    let errorNum = document.querySelector("#error-num");
    let errorPass = document.querySelector("#error-pass");
    let errorCpass = document.querySelector("#error-cpass");

    if (name == "") {
        errorName.innerText = "please enter your name!";
        errorName.style.visibility = "visible";
        return false;
    } else {
        errorName.style.visibility = "hidden";
    }

    if (email == "") {
        errorEmail.innerText = "please enter your email!";
        errorEmail.style.visibility = 'visible';
        return false;
    } else {
        errorEmail.style.visibility = 'hidden';
    }

    if (!(email.includes("@") && email.includes(".com"))) {
        errorEmail.innerText = "please enter valid email!";
        errorEmail.style.visibility = "visible";
        return false;
    } else {
        errorEmail.style.visibility = "hidden";
    }

    if (num == "") {
        errorNum.innerText = "please enter your number!";
        errorNum.style.visibility = "visible";
        return false;
    } else {
        errorNum.style.visibility = "hidden";
    }

    if (isNaN(num)) {
        errorNum.innerText = "please enter valid number";
        errorNum.style.visibility = "visible";
        return false;
    } else {
        errorNum.style.visibility = "hidden";
    }

    if (num.length != 10) {
        errorNum.innerText = "please enter valid number";
        errorNum.style.visibility = "visible";
        return false;
    } else {
        errorNum.style.visibility = "hidden";
    }

    if (password == "") {
        errorPass.innerText = "please enter your password!";
        errorPass.style.visibility = "visible";
        return false;
    } else {
        errorPass.style.visibility = "hidden";
    }

    if (password != confirmPassword) {
        errorCpass.innerText = "confirm password not valid!";
        errorCpass.style.visibility = "visible";
        return false;
    } else {
        errorCpass.style.visibility = "hidden";
    }

    if (!(password.match(/[@#$%&]/) && password.match(/[0-9]/) && password.match(/[a-z]/))) {
        errorCpass.innerText = "please enter strong password!";
        errorCpass.style.visibility = "visible";
        return false;
    } else {
        errorCpass.style.visibility = "hidden";
    }

    // Save to localStorage
    let user = {
        name: name,
        email: email,
        number: num,
        password: password
    };

    localStorage.setItem("flynowUser", JSON.stringify(user));

    // Show login form
    showLogin();

    // SweetAlert2 Success Message
    alert("Signup Successful!")

    return false;
}



let loginForm = () => {
    let email = document.querySelector("#login-email").value.trim();
    let password = document.querySelector("#login-password").value.trim();

    let errorLogin = document.querySelector("#error-login");
    let errorLogin1 = document.querySelector("#error-login1");

    let savedUser = JSON.parse(localStorage.getItem("flynowUser"));
    // console.log(savedUser);

    // Agar user exist hi nahi karta
    if (!savedUser) {
        alert("Please signup first")
        return;
    }

    // Email validation
    if (email === "") {
        errorLogin.innerText = "Please enter your email!";
        errorLogin.style.visibility = "visible";
        return;
    } else if (!(email.includes("@") && email.includes(".com"))) {
        errorLogin.innerText = "Please enter valid email!";
        errorLogin.style.visibility = "visible";
        return;
    } else {
        errorLogin.style.visibility = "hidden";
    }

    // Password validation
    if (password === "") {
        errorLogin1.innerText = "Please enter your password!";
        errorLogin1.style.visibility = "visible";
        return;
    } else {
        errorLogin1.style.visibility = "hidden";
    }

    // Login check
    if (email === savedUser.email && password === savedUser.password) {
        alert("Login Succesfull")
        localStorage.setItem("isLoggedIn", "true");
        localStorage.setItem("loggedInUser", JSON.stringify(savedUser));

        closeAuth();
        updateNavbar();

    } else {
        alert("Invalid email or password. Please try again!")
    }
};

let updateNavbar = () => {
    // Sunucu tarafı auth kullanıldığında bu elementler DOM'da olmayabilir - null-safe
    let logoutEl = document.querySelector("#Logout");
    let afterEl = document.querySelector(".after");
    let after1El = document.querySelector(".after1");
    let after2El = document.querySelector(".after2");
    let userInfoEl = document.querySelector(".user-info");

    let isLoggedIn = localStorage.getItem("isLoggedIn");
    let loggedInUser = JSON.parse(localStorage.getItem("loggedInUser"));

    if (isLoggedIn === "true" && loggedInUser && (logoutEl || afterEl)) {
        if (logoutEl) { logoutEl.style.display = "block"; logoutEl.innerText = "Logout"; }
        if (afterEl) afterEl.style.display = "none";
        if (after1El) after1El.style.display = "none";
        if (after2El) after2El.style.display = "none";
        if (userInfoEl) userInfoEl.innerHTML = `Welcome, <span style="color: #0B8CD3;">${loggedInUser.name}!</span>`;
    } else if (afterEl) {
        if (logoutEl) logoutEl.style.display = "none";
        if (afterEl) afterEl.style.display = "block";
        if (after1El) after1El.style.display = "block";
        if (after2El) after2El.style.display = "block";
    }
}

let logOutUser = () => {
    localStorage.removeItem("isLoggedIn")
    localStorage.removeItem("loggedInUser")
    localStorage.removeItem("flynowUser")
    updateNavbar()
    let logoutNav = document.querySelector("#Logout-nav");
    let loginNav = document.querySelector(".login-nav");
    let loginNav1 = document.querySelector(".login-nav1");
    let userInfo = document.querySelector(".user-info");
    if (logoutNav) logoutNav.style.display = "none";
    if (loginNav) loginNav.style.display = "block";
    if (loginNav1) loginNav1.style.display = "block";
    if (userInfo) userInfo.style.display = "none";
}

let showNavbar = () => {
    let navLinks = document.querySelector("#nav-links");
    let navWrap = document.querySelector(".navbar-nav-wrap");
    if (navLinks) navLinks.classList.toggle("active");
    if (navWrap) navWrap.classList.toggle("mobile-open");

    let loginNav = document.querySelector(".login-nav");
    let loginNav1 = document.querySelector(".login-nav1");
    let logoutNav = document.querySelector("#Logout-nav");

    let isLoggedIn = localStorage.getItem("isLoggedIn");
    let loggedInUser = JSON.parse(localStorage.getItem("loggedInUser"));

    if (isLoggedIn === "true" && loggedInUser) {
        if (logoutNav) logoutNav.style.display = "block";
        if (loginNav) loginNav.style.display = "none";
        if (loginNav1) loginNav1.style.display = "none";
    } else {
        if (logoutNav) logoutNav.style.display = "none";
        if (loginNav) loginNav.style.display = "block";
        if (loginNav1) loginNav1.style.display = "block";
    }
}


// booking ticket 

let bookTicket = () => {
    let full_name = document.querySelector("#fullname").value.trim();
    let gender = document.querySelector("#gender").value.trim();
    let email = document.querySelector("#booking_email").value.trim();
    let contact = document.querySelector("#contact").value.trim();
    let nationality = document.querySelector("#nationality").value.trim();
    let flight_no = document.querySelector("#flightno").value.trim();
    let from = document.querySelector("#from").value.trim();
    let to = document.querySelector("#to").value.trim();
    let date = document.querySelector("#date").value.trim();
    let price = document.querySelector("#price").value.trim();

    // Simple validation
    if (!full_name || !gender || !email || !contact || !nationality || !flight_no || !from || !to || !date || !price) {
        alert("Please fill all the fields.");
        return false;
    }

    // Basic email check
    if (email.indexOf("@") === -1 || email.indexOf(".") === -1) {
        alert("Please enter a valid email.");
        return false;
    }

    // Contact check (basic 10 digits)
    if (contact.length !== 10 || isNaN(contact)) {
        alert("Please enter a valid 10-digit contact number.");
        return false;
    }

    // Price check
    if (isNaN(price) || Number(price) <= 0) {
        alert("Please enter a valid price.");
        return false;
    }

    // Simple future date check
    let selectedDate = new Date(date);
    let today = new Date();
    today.setHours(0, 0, 0, 0);
    if (selectedDate < today) {
        alert("Departure date cannot be in the past.");
        return false;
    }

    // Check if user is logged in
    let savedUser = JSON.parse(localStorage.getItem("flynowUser"));
    if (!savedUser) {
        alert("Login first");
        return false;
    }

    let url = 'http://localhost:3000/Ticket'
    // Submit using fetch (simple)
    fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            "Full_Name": full_name,
            "Gender": gender,
            "Email": email,
            "Contact": contact,
            "Nationality": nationality,
            "Flight_no": flight_no,
            "From": from,
            "To": to,
            "Date": date,
            "Price": price
        })
    });

    alert("Ticket booked successfully!");
    return false; // form submit roko
};



let showTicket = async () => {
    let url = 'http://localhost:3000/Ticket'

    let res = await fetch(url, { method: "GET" })

    let data = await res.json()

    // console.log(data[0].Full_Name)

    let savedUser = localStorage.getItem("isLoggedIn")
    console.log(savedUser)

if(savedUser){
    data.map((e) => {
        document.querySelector(".my_booking_ticket").innerHTML += `
        <div class="ticket-card">
            <div class="ticket-header">
                <h2>Flight Ticket</h2>
                <span>#AI202</span>
            </div>

            <div class="ticket-body">
                <div class="ticket-row">
                    <span class="ticket-label">Full Name:</span>
                    <span class="ticket-value">${e.Full_Name}</span>
                </div>
                <div class="ticket-row">
                    <span class="ticket-label">Gender:</span>
                    <span class="ticket-value">${e.Gender}</span>
                </div>
                <div class="ticket-row">
                    <span class="ticket-label">Email:</span>
                    <span class="ticket-value">${e.Email}</span>
                </div>
                <div class="ticket-row">
                    <span class="ticket-label">Contact:</span>
                    <span class="ticket-value">${e.Contact}</span>
                </div>
                <div class="ticket-row">
                    <span class="ticket-label">Nationality:</span>
                    <span class="ticket-value">${e.Nationality}</span>
                </div>
                <div class="ticket-row">
                    <span class="ticket-label">From &rarr; To:</span>
                    <span class="ticket-value">${e.From} &rarr; ${e.To}</span>
                </div>
                <div class="ticket-row">
                    <span class="ticket-label">Date:</span>
                    <span class="ticket-value">${e.Date}</span>
                </div>
                <div class="ticket-row">
                    <span class="ticket-label">Price:</span>
                    <span class="ticket-value">${e.Price}</span>
                </div>
            </div>

            <div class="ticket-actions">
                <button class="btn btn-edit" onclick="opneEditForm('${e.id}')"> Edit</button>
                <button class="btn btn-delete" onclick="Del('${e.id}')"> Delete</button>
            </div>
        </div>
        `
    })
}}

let opneEditForm = async (id) => {

    let edit_form = document.querySelector(".flight_booking_form")

    let url =  `http://localhost:3000/Ticket/${id}`

    let res = await fetch(url,{method:"GET"})

    let user_data = await res.json()

    console.log(user_data)

    edit_form.innerHTML = `
        <div class="card-form-flight-booking">
            <h2>Flight Booking Form</h2>
            <form>
                <div class="form-row-flight-booking">
                    <div class="form-group">
                        <label for="fullname">Full Name</label>
                        <input type="text" id="fullname" placeholder="Enter your full name" value="${user_data.Full_Name}">
                    </div>

                    <div class="form-group">
                        <label for="gender">Gender</label>
                        <select id="gender">
                            <option value="">Select Gender</option>
                            <option value="male">Male</option>
                            <option value="female">Female</option>
                            <option value="other">Other</option>
                        </select>
                    </div>
                </div>

                <div class="form-row-flight-booking">
                    <div class="form-group">
                        <label for="booking_email">Email</label>
                        <input type="email" id="booking_email" placeholder="Enter your email" value="${user_data.Email}">
                    </div>

                    <div class="form-group">
                        <label for="contact">Contact</label>
                        <input type="tel" id="contact" placeholder="Enter your contact number" value="${user_data.Contact}">
                    </div>
                </div>

                <div class="form-row-flight-booking">
                    <div class="form-group">
                        <label for="nationality">Nationality</label>
                        <select id="nationality">
                            <option value="">Select Nationality</option>
                            <option value="indian">Indian</option>
                            <option value="american">American</option>
                            <option value="british">British</option>
                            <option value="other">Other</option>
                        </select>
                    </div>

                    <div class="form-group">
                        <label for="flightno">Flight Number</label>
                        <input type="text" id="flightno" placeholder="Enter flight number" value="${user_data.Flight_no}">
                    </div>
                </div>

                <div class="form-row-flight-booking">
                    <div class="form-group">
                        <label for="from">From</label>
                        <input type="text" id="from" placeholder="From (City)" value="${user_data.From}">
                    </div>

                    <div class="form-group">
                        <label for="to">To</label>
                        <input type="text" id="to" placeholder="To (City)" value="${user_data.To}">
                    </div>

                    <div class="form-group">
                        <label for="date">Departure</label>
                        <input type="date" id="date" placeholder="Date" value="${user_data.Date}">
                    </div>
                    
                    <div class="form-group">
                        <div>
                            <label for="price">Price</label>
                            <input type="text" id="price" placeholder="Enter Amount" value="${user_data.Price}">
                        </div>
                    </div>
                </div>

                <div style="text-align: center;">
                    <button type="submit" class="btn-submit" onclick="return updateTicket('${user_data.id}')">Book</button>
                </div>
            </form>
        </div>
        `
    }


    let updateTicket = (id) => {
    let full_name = document.querySelector("#fullname").value.trim();
    let gender = document.querySelector("#gender").value.trim();
    let email = document.querySelector("#booking_email").value.trim();
    let contact = document.querySelector("#contact").value.trim();
    let nationality = document.querySelector("#nationality").value.trim();
    let flight_no = document.querySelector("#flightno").value.trim();
    let from = document.querySelector("#from").value.trim();
    let to = document.querySelector("#to").value.trim();
    let date = document.querySelector("#date").value.trim();
    let price = document.querySelector("#price").value.trim();

    // Simple validation
    if (!full_name || !gender || !email || !contact || !nationality || !flight_no || !from || !to || !date || !price) {
        alert("Please fill all the fields.");
        return false;
    }

    // Basic email check
    if (email.indexOf("@") === -1 || email.indexOf(".") === -1) {
        alert("Please enter a valid email.");
        return false;
    }

    // Contact check (basic 10 digits)
    if (contact.length !== 10 || isNaN(contact)) {
        alert("Please enter a valid 10-digit contact number.");
        return false;
    }

    // Price check
    if (isNaN(price) || Number(price) <= 0) {
        alert("Please enter a valid price.");
        return false;
    }

    // Simple future date check
    let selectedDate = new Date(date);
    let today = new Date();
    today.setHours(0, 0, 0, 0);
    if (selectedDate < today) {
        alert("Departure date cannot be in the past.");
        return false;
    }

    // Check if user is logged in
    let savedUser = JSON.parse(localStorage.getItem("flynowUser"));
    if (!savedUser) {
        alert("Login first");
        return false;
    }

    let url = `http://localhost:3000/Ticket/${id}`
    // update Submit using fetch (simple)
    fetch(url, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            "Full_Name": full_name,
            "Gender": gender,
            "Email": email,
            "Contact": contact,
            "Nationality": nationality,
            "Flight_no": flight_no,
            "From": from,
            "To": to,
            "Date": date,
            "Price": price
        })
    });

    alert("Ticket Details update successfully!");
    return false; // form submit roko
};

let Del = (id) => {
    let url = `http://localhost:3000/Ticket/${id}`
    fetch(url,{method:"DELETE"})
}

let openBookPage = () => {
    var bookingSection = document.getElementById('booking-section') || document.querySelector('.booking-section-home');
    if (bookingSection) {
        bookingSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
    } else {
        location.href = "/Flight/Listing";
    }
}

let showFlight = () =>{
    var form = document.querySelector('.booking-form');
    if (form) {
        form.submit();
    } else {
        location.href = "/Flight/Listing";
    }
}

window.onload = () => {
    showTicket()
    updateNavbar()
}