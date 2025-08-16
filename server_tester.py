import requests

def main():
    global amount, url, header
    url = input("url:")
    header = input("include headers? (y/n): ")
    if url == "":
        url = "http://localhost:4221/"
        print(f"Using URL: {url}")
    else:
        url = url.strip()
    amount = int(input("number of requests: "))
    requestType = input("GET or POST? (g/p): ").lower()
    if requestType == "g":
        GET()
    elif requestType == "p":
        POST()
    else:
        print("invalid input")
        return

def GET():
    try:
        for i in range(amount):
            with requests.Session() as session:
                get = session.get(url, headers={"Connection": "close"})
                if get.status_code == 200:
                    if header.lower() == "y":
                        print(f"\nGET request successful (code: {get.status_code})\n")
                        print(f"Body:\n {get.text}")
                    else:
                        print(f"\nGET request successful (code: {get.status_code})\n")
                else:
                    print(f"GET request unsuccessful (code: {get.status_code})")
    except requests.exceptions.RequestException as e:
        print(f"An error occurred: {e}")

def POST():
    try:
        for i in range(amount):
            with requests.Session() as session:
                post = session.post(url, headers={"Connection": "close"})
                if post.status_code == 200:
                    if header.lower() == "y":
                        print(f"\nPOST request successful (code: {post.status_code})\n")
                        print(f"Body:\n {post.text}")
                    else:
                        print(f"\nPOST request successful (code: {post.status_code})\n")
                else:
                    print(f"POST request unsuccessful (code: {post.status_code})")
    except requests.exceptions.RequestException as e:
        print(f"An error occurred: {e}")


main()